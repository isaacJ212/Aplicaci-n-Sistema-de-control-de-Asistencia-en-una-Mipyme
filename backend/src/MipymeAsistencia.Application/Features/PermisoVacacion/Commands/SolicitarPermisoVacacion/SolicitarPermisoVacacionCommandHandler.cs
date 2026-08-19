using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Commands.SolicitarPermisoVacacion;

public class SolicitarPermisoVacacionCommandHandler : IRequestHandler<SolicitarPermisoVacacionCommand, PermisoVacacionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public SolicitarPermisoVacacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermisoVacacionResponseDto> Handle(SolicitarPermisoVacacionCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        if (request.FechaInicio > request.FechaFin)
            throw new InvalidOperationException("La fecha de inicio no puede ser mayor que la fecha final.");

        var tipo = request.TipoSolicitud.Trim();
        var tiposValidos = new[] { "Vacaciones", "Permiso Medico", "Permiso Personal", "Permiso", "Vacacion" };
        if (!tiposValidos.Contains(tipo))
            throw new InvalidOperationException("El tipo de solicitud debe ser 'Vacaciones', 'Permiso Medico' o 'Permiso Personal'.");

        // Normalizar alias cortos al valor canónico
        if (tipo == "Vacacion") tipo = "Vacaciones";
        if (tipo == "Permiso")  tipo = "Permiso Personal";

        // ─── Determinar si la solicitud es POR HORAS o POR DÍAS ─────────────
        bool esPorHoras = request.HorasSolicitadas.HasValue
                          && request.HorasSolicitados.Value > 0m
                          && (!request.DiasSolicitados.HasValue || request.DiasSolicitados.Value == 0m);

        // Para solicitudes por horas, FechaInicio y FechaFin deben ser el MISMO día
        if (esPorHoras && request.FechaInicio.Date != request.FechaFin.Date)
            throw new InvalidOperationException("Las solicitudes por horas deben iniciar y finalizar el mismo día.");

        // Validar HorasSolicitadas contra jornada diaria (8h por defecto desde ConfiguracionSede)
        decimal? horasSolicitadas = null;
        decimal diasSolicitados;
        string unidadTiempo;

        if (esPorHoras)
        {
            horasSolicitadas = request.HorasSolicitadas!.Value;
            if (horasSolicitadas <= 0m)
                throw new InvalidOperationException("Las horas solicitadas deben ser mayores a 0.");

            // Obtener jornada máxima diaria de la sede (default 8h)
            var jornadaHoras = 8m;
            var sede = await _context.ConfiguracionesSede
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (sede is not null)
            {
                var hs = (sede.HoraSalidaOficial - sede.HoraEntradaOficial).TotalMinutes;
                hs = Math.Max(0, hs - sede.DuracionAlmuerzoMinutos);
                jornadaHoras = (decimal)Math.Round(hs / 60.0, 2);
            }

            if (horasSolicitadas > jornadaHoras)
                throw new InvalidOperationException(
                    $"No se pueden solicitar más de {jornadaHoras} horas por día (jornada diaria).");

            // Para permisos por horas, días = 0 (fracción de día)
            diasSolicitados = 0m;
            unidadTiempo = "Horas";
        }
        else
        {
            diasSolicitados = request.DiasSolicitados ?? CalcularDiasEntreFechas(request.FechaInicio, request.FechaFin);
            if (diasSolicitados <= 0)
                throw new InvalidOperationException("La cantidad de días solicitados debe ser mayor a cero.");
            unidadTiempo = "Dias";
        }

        // ─── Validaciones específicas por tipo ──────────────────────────────
        if (tipo == "Vacaciones")
        {
            if (esPorHoras)
                throw new InvalidOperationException("Las vacaciones no se pueden solicitar por horas, solo por días completos.");

            if (empleado.DiasVacacionesAcumuladas < diasSolicitados)
                throw new InvalidOperationException("El empleado no tiene suficientes días de vacaciones acumulados.");
        }

        // ─── Verificar que no existan solicitudes solapadas ────────────────
        var inicio = NormalizeToUtcDate(request.FechaInicio);
        var fin    = NormalizeToUtcDate(request.FechaFin);

        bool solapado = await _context.HistorialPermisosVacaciones
            .AnyAsync(p =>
                p.IdEmpleado == request.IdEmpleado
                && p.EstadoSolicitud != "Rechazado"
                && p.FechaInicio <= fin
                && p.FechaFin    >= inicio,
                cancellationToken);

        if (solapado)
            throw new InvalidOperationException(
                "Ya existe una solicitud aprobada/pendiente en ese rango de fechas.");

        var solicitud = new HistorialPermisoVacacion
        {
            IdEmpleado = request.IdEmpleado,
            TipoSolicitud = tipo,
            FechaInicio = inicio,
            FechaFin = fin,
            DiasSolicitados = diasSolicitados,
            HorasSolicitadas = horasSolicitadas,
            Motivo = request.Motivo.Trim(),
            EstadoSolicitud = "Pendiente",
            FechaRespuesta = null,
            IdUsuarioAprobador = null
        };

        _context.HistorialPermisosVacaciones.Add(solicitud);
        await _context.SaveChangesAsync(cancellationToken);

        return new PermisoVacacionResponseDto
        {
            IdSolicitud = solicitud.IdSolicitud,
            IdEmpleado = solicitud.IdEmpleado,
            NombreEmpleado = $"{empleado.Nombres} {empleado.Apellidos}".Trim(),
            TipoSolicitud = solicitud.TipoSolicitud,
            FechaInicio = solicitud.FechaInicio,
            FechaFin = solicitud.FechaFin,
            DiasSolicitados = solicitud.DiasSolicitados,
            HorasSolicitadas = solicitud.HorasSolicitadas,
            UnidadTiempo = unidadTiempo,
            Motivo = solicitud.Motivo,
            EstadoSolicitud = solicitud.EstadoSolicitud,
            FechaRespuesta = solicitud.FechaRespuesta,
            IdUsuarioAprobador = solicitud.IdUsuarioAprobador
        };
    }

    private static decimal CalcularDiasEntreFechas(DateTime fechaInicio, DateTime fechaFin)
    {
        var diferencia = fechaFin.Date - fechaInicio.Date;
        return Math.Max(1, diferencia.Days + 1);
    }

    private static DateTime NormalizeToUtcDate(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.Date,
            DateTimeKind.Local => value.ToUniversalTime().Date,
            _ => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc)
        };
    }
}
