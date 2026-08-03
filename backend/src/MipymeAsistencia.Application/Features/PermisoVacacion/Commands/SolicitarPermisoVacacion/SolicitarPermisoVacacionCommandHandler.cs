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
        if (tipo != "Permiso" && tipo != "Vacacion")
            throw new InvalidOperationException("El tipo de solicitud debe ser 'Permiso' o 'Vacacion'.");

        var diasSolicitados = request.DiasSolicitados ?? CalcularDiasEntreFechas(request.FechaInicio, request.FechaFin);

        if (diasSolicitados <= 0)
            throw new InvalidOperationException("La cantidad de días solicitados debe ser mayor a cero.");

        if (tipo == "Vacacion" && empleado.DiasVacacionesAcumuladas < diasSolicitados)
            throw new InvalidOperationException("El empleado no tiene suficientes días de vacaciones acumulados.");

        var solicitud = new HistorialPermisoVacacion
        {
            IdEmpleado = request.IdEmpleado,
            TipoSolicitud = tipo,
            FechaInicio = NormalizeToUtcDate(request.FechaInicio),
            FechaFin = NormalizeToUtcDate(request.FechaFin),
            DiasSolicitados = diasSolicitados,
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
