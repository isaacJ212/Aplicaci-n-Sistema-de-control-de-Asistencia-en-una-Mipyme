using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.AcumularVacaciones;

/// <summary>
/// Recalcula DiasVacacionesAcumuladas para un empleado según:
///   1. Meses completos desde FechaContratacion → 2.5 días/mes
///   2. Resta los días tomados como vacaciones (solicitudes Aprobadas)
///   3. Actualiza el campo en la BD
/// </summary>
public class AcumularVacacionesCommandHandler
    : IRequestHandler<AcumularVacacionesCommand, AcumularVacacionesResponseDto>
{
    private readonly IApplicationDbContext _context;

    public AcumularVacacionesCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<AcumularVacacionesResponseDto> Handle(
        AcumularVacacionesCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken)
            ?? throw new KeyNotFoundException("El empleado no existe.");

        var hoy              = DateTime.UtcNow.Date;
        var fechaContratacion = empleado.FechaContratacion.Date;

        if (fechaContratacion > hoy)
            throw new InvalidOperationException("La fecha de contratación es posterior a hoy.");

        // ── 1. Meses completos trabajados ─────────────────────────────────────
        int mesesTrabajados = ((hoy.Year - fechaContratacion.Year) * 12)
                            + (hoy.Month - fechaContratacion.Month);
        // Si el día del mes actual no llegó al día de contratación, restar 1
        if (hoy.Day < fechaContratacion.Day) mesesTrabajados--;
        mesesTrabajados = Math.Max(0, mesesTrabajados);

        // ── 2. Días acumulados teóricos (2.5 × meses) ────────────────────────
        const decimal tasaMensual = 2.5m;
        var diasAcumuladosTeoricos = mesesTrabajados * tasaMensual;

        // ── 3. Días reales trabajados (registros de asistencia con entrada) ───
        var inicioContrato = DateTime.SpecifyKind(fechaContratacion, DateTimeKind.Utc);
        int diasTrabajadosReales = await _context.HistorialAsistencias
            .CountAsync(h => h.IdEmpleado == request.IdEmpleado
                          && h.Fecha >= inicioContrato
                          && h.HoraEntrada != default,
                        cancellationToken);

        // ── 4. Días ya descontados: vacaciones aprobadas ──────────────────────
        var diasDescontados = await _context.HistorialPermisosVacaciones
            .Where(p => p.IdEmpleado   == request.IdEmpleado
                     && p.TipoSolicitud == "Vacaciones"
                     && p.EstadoSolicitud == "Aprobado")
            .SumAsync(p => p.DiasSolicitados, cancellationToken);

        // ── 5. Saldo disponible ───────────────────────────────────────────────
        var saldoDisponible = Math.Max(0m, diasAcumuladosTeoricos - diasDescontados);

        empleado.DiasVacacionesAcumuladas = saldoDisponible;
        await _context.SaveChangesAsync(cancellationToken);

        return new AcumularVacacionesResponseDto
        {
            IdEmpleado                = empleado.IdEmpleado,
            NombreEmpleado            = $"{empleado.Nombres} {empleado.Apellidos}".Trim(),
            FechaContratacion         = empleado.FechaContratacion,
            MesesTrabajados           = mesesTrabajados,
            DiasTrabajadosReales      = diasTrabajadosReales,
            DiasAcumuladosTeoricos    = diasAcumuladosTeoricos,
            DiasDescontadosVacaciones = diasDescontados,
            DiasVacacionesDisponibles = saldoDisponible,
        };
    }
}
