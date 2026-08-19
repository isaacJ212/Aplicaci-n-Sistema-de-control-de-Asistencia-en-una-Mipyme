using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PeriodoCierreEntity = MipymeAsistencia.Domain.Entities.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CrearPeriodoCierre;

public class CrearPeriodoCierreCommandHandler : IRequestHandler<CrearPeriodoCierreCommand, PeriodoCierreDto>
{
    private readonly IApplicationDbContext _context;

    public CrearPeriodoCierreCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<PeriodoCierreDto> Handle(CrearPeriodoCierreCommand request, CancellationToken cancellationToken)
    {
        var periodoNorm = request.Periodo.Trim();
        if (periodoNorm.Length != 7 || !periodoNorm.Contains('-'))
            throw new InvalidOperationException("El formato del periodo debe ser YYYY-MM (ej. 2026-08).");

        var existente = await _context.PeriodosCierrePlanilla
            .FirstOrDefaultAsync(p => p.Periodo == periodoNorm, cancellationToken);

        if (existente != null)
        {
            if (existente.Cerrado)
                throw new InvalidOperationException($"El periodo {periodoNorm} ya se encuentra cerrado definitivamente.");

            existente.FechaCorteHorasExtras = request.FechaCorteHorasExtras.ToUniversalTime();
            existente.FechaEmisionPlanilla  = request.FechaEmisionPlanilla.ToUniversalTime();
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
                existente.Observaciones = request.Observaciones.Trim();

            await _context.SaveChangesAsync(cancellationToken);

            return new PeriodoCierreDto
            {
                IdPeriodoCierre       = existente.IdPeriodoCierre,
                Periodo               = existente.Periodo,
                FechaCorteHorasExtras = existente.FechaCorteHorasExtras,
                FechaEmisionPlanilla  = existente.FechaEmisionPlanilla,
                Cerrado               = existente.Cerrado,
                FechaCierreDefinitivo = existente.FechaCierreDefinitivo,
                IdUsuarioCierre       = existente.IdUsuarioCierre,
                Observaciones         = existente.Observaciones
            };
        }

        var nuevo = new PeriodoCierreEntity
        {
            Periodo               = periodoNorm,
            FechaCorteHorasExtras = request.FechaCorteHorasExtras.ToUniversalTime(),
            FechaEmisionPlanilla  = request.FechaEmisionPlanilla.ToUniversalTime(),
            Cerrado               = false,
            Observaciones         = request.Observaciones?.Trim()
        };

        _context.PeriodosCierrePlanilla.Add(nuevo);
        await _context.SaveChangesAsync(cancellationToken);

        return new PeriodoCierreDto
        {
            IdPeriodoCierre       = nuevo.IdPeriodoCierre,
            Periodo               = nuevo.Periodo,
            FechaCorteHorasExtras = nuevo.FechaCorteHorasExtras,
            FechaEmisionPlanilla  = nuevo.FechaEmisionPlanilla,
            Cerrado               = nuevo.Cerrado,
            FechaCierreDefinitivo = null,
            IdUsuarioCierre       = null,
            Observaciones         = nuevo.Observaciones
        };
    }
}
