using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodoCierreByPeriodo;

public class GetPeriodoCierreByPeriodoQueryHandler : IRequestHandler<GetPeriodoCierreByPeriodoQuery, PeriodoCierreDto>
{
    private readonly IApplicationDbContext _context;

    public GetPeriodoCierreByPeriodoQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<PeriodoCierreDto> Handle(GetPeriodoCierreByPeriodoQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.PeriodosCierrePlanilla
            .Include(x => x.UsuarioCierre)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Periodo == request.Periodo.Trim(), cancellationToken);

        if (p is null)
            throw new KeyNotFoundException($"No se encontró configuración de cierre para el periodo '{request.Periodo}'.");

        return new PeriodoCierreDto
        {
            IdPeriodoCierre       = p.IdPeriodoCierre,
            Periodo               = p.Periodo,
            FechaCorteHorasExtras = p.FechaCorteHorasExtras,
            FechaEmisionPlanilla  = p.FechaEmisionPlanilla,
            Cerrado               = p.Cerrado,
            FechaCierreDefinitivo = p.FechaCierreDefinitivo,
            IdUsuarioCierre       = p.IdUsuarioCierre,
            EmailUsuarioCierre    = p.UsuarioCierre?.Email,
            Observaciones         = p.Observaciones
        };
    }
}
