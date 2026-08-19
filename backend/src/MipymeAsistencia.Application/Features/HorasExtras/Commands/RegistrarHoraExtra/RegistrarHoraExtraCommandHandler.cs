using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.HorasExtras.Commands.RegistrarHoraExtra;

public class RegistrarHoraExtraCommandHandler
    : IRequestHandler<RegistrarHoraExtraCommand, HoraExtraResponseDto>
{
    private readonly IApplicationDbContext _context;

    public RegistrarHoraExtraCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<HoraExtraResponseDto> Handle(
        RegistrarHoraExtraCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException($"Empleado con id {request.IdEmpleado} no encontrado.");

        var fechaUtc = request.Fecha.ToUniversalTime();
        var fechaDate = fechaUtc.Date;
        var periodoStr = fechaUtc.ToString("yyyy-MM");

        // Validar si el periodo de planilla está cerrado o ya pasó la fecha de corte
        var periodoCierre = await _context.PeriodosCierrePlanilla
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Periodo == periodoStr, cancellationToken);

        if (periodoCierre != null)
        {
            if (periodoCierre.Cerrado)
                throw new InvalidOperationException(
                    $"No se pueden registrar horas extras para el periodo {periodoStr} porque ya se encuentra cerrado.");

            if (fechaUtc > periodoCierre.FechaCorteHorasExtras)
                throw new InvalidOperationException(
                    $"La fecha de la hora extra ({fechaUtc:yyyy-MM-dd}) supera la fecha límite de corte ({periodoCierre.FechaCorteHorasExtras:yyyy-MM-dd}) para el periodo {periodoStr}.");
        }

        // Obtener parámetro de horas laborales al mes (default 240)
        var paramHorasMes = await _context.ParametrosLaborales
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Clave == "HORAS_LABORALES_MES", cancellationToken);
        var horasLaboralesMes = paramHorasMes != null && paramHorasMes.Valor > 0 ? paramHorasMes.Valor : 240m;

        // Validar si la fecha de la hora extra es un día feriado o descanso semanal (domingo)
        var feriado = await _context.DiasFeriados
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Fecha.Date == fechaDate, cancellationToken);

        var esFeriadoRecuperable = feriado != null && feriado.EsRecuperable;
        var esDomingoDescanso    = fechaUtc.DayOfWeek == DayOfWeek.Sunday;

        // Código del Trabajo Nicaragua Arto. 62:
        // En feriados y días de descanso el trabajo extraordinario se remunera con factor 2.0 (pago doble).
        var factorFinal = request.FactorRecargo;
        if ((esFeriadoRecuperable || esDomingoDescanso) && factorFinal < 2.0m)
        {
            factorFinal = 2.0m;
        }

        // Fórmula Arto. 62 Ley 185 Nicaragua:
        // MontoPagar = (SalarioMensual / horasLaboralesMes) * FactorRecargo * CantidadHoras
        var montoHora  = empleado.SalarioBaseMensual / horasLaboralesMes;
        var montoPagar = Math.Round(montoHora * factorFinal * request.CantidadHoras, 2);

        var motivoFinal = request.Motivo;
        if (feriado != null && !motivoFinal.Contains($"[Feriado: {feriado.Nombre}]", StringComparison.OrdinalIgnoreCase))
        {
            motivoFinal = $"{motivoFinal} [Feriado: {feriado.Nombre} - Factor 2.0x]".Trim();
        }

        var horaExtra = new HoraExtra
        {
            IdEmpleado    = request.IdEmpleado,
            Fecha         = fechaUtc,
            CantidadHoras = request.CantidadHoras,
            Motivo        = motivoFinal,
            MontoPagar    = montoPagar,
            Estado        = "Pendiente"          // Inicia pendiente de aprobación
        };

        _context.HorasExtras.Add(horaExtra);
        await _context.SaveChangesAsync(cancellationToken);

        return new HoraExtraResponseDto
        {
            IdHoraExtra        = horaExtra.IdHoraExtra,
            IdEmpleado         = horaExtra.IdEmpleado,
            NombreEmpleado     = empleado.Nombres + " " + empleado.Apellidos,
            IdUsuarioAprobador = null,
            NombreAprobador    = null,
            Fecha              = horaExtra.Fecha,
            CantidadHoras      = horaExtra.CantidadHoras,
            Motivo             = horaExtra.Motivo,
            MontoPagar         = horaExtra.MontoPagar,
            Estado             = horaExtra.Estado
        };
    }
}
