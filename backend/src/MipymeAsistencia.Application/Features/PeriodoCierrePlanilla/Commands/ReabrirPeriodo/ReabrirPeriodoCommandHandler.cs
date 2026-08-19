using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.ReabrirPeriodo;

public class ReabrirPeriodoCommandHandler : IRequestHandler<ReabrirPeriodoCommand, PeriodoCierreDto>
{
    private readonly IApplicationDbContext _context;

    public ReabrirPeriodoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<PeriodoCierreDto> Handle(ReabrirPeriodoCommand request, CancellationToken cancellationToken)
    {
        var periodoNorm = request.Periodo.Trim();

        var periodo = await _context.PeriodosCierrePlanilla
            .Include(p => p.UsuarioCierre)
            .FirstOrDefaultAsync(p => p.Periodo == periodoNorm, cancellationToken);

        if (periodo is null)
            throw new KeyNotFoundException($"No se encontró el periodo '{periodoNorm}'.");

        if (!periodo.Cerrado)
            throw new InvalidOperationException($"El periodo '{periodoNorm}' ya se encuentra abierto.");

        periodo.Cerrado = false;
        periodo.FechaCierreDefinitivo = null;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
        {
            periodo.Observaciones = $"{periodo.Observaciones} | Reabierto: {request.Motivo.Trim()}".Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PeriodoCierreDto
        {
            IdPeriodoCierre       = periodo.IdPeriodoCierre,
            Periodo               = periodo.Periodo,
            FechaCorteHorasExtras = periodo.FechaCorteHorasExtras,
            FechaEmisionPlanilla  = periodo.FechaEmisionPlanilla,
            Cerrado               = periodo.Cerrado,
            FechaCierreDefinitivo = periodo.FechaCierreDefinitivo,
            IdUsuarioCierre       = periodo.IdUsuarioCierre,
            EmailUsuarioCierre    = periodo.UsuarioCierre?.Email,
            Observaciones         = periodo.Observaciones
        };
    }
}
