using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodosCierre;

public class GetPeriodosCierreQueryHandler : IRequestHandler<GetPeriodosCierreQuery, List<PeriodoCierreDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPeriodosCierreQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<PeriodoCierreDto>> Handle(GetPeriodosCierreQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PeriodosCierrePlanilla
            .Include(p => p.UsuarioCierre)
            .AsNoTracking();

        if (request.SoloAbiertos.HasValue && request.SoloAbiertos.Value)
        {
            query = query.Where(p => !p.Cerrado);
        }

        var list = await query
            .OrderByDescending(p => p.Periodo)
            .ToListAsync(cancellationToken);

        return list.Select(p => new PeriodoCierreDto
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
        }).ToList();
    }
}
