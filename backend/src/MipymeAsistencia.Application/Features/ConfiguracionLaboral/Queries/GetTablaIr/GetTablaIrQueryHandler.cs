using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetTablaIr;

public class GetTablaIrQueryHandler : IRequestHandler<GetTablaIrQuery, List<TablaIrDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTablaIrQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<TablaIrDto>> Handle(GetTablaIrQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TablaImpuestoRenta.AsNoTracking();

        if (request.SoloActivos)
        {
            query = query.Where(t => t.Activo);
        }

        if (request.Anio.HasValue)
        {
            query = query.Where(t => t.AnioVigencia == request.Anio.Value);
        }

        var tramos = await query
            .OrderBy(t => t.DesdeMontoAnual)
            .ToListAsync(cancellationToken);

        return tramos.Select(t => new TablaIrDto
        {
            IdTablaIr           = t.IdTablaIr,
            DesdeMontoAnual     = t.DesdeMontoAnual,
            HastaMontoAnual     = t.HastaMontoAnual,
            PorcentajeAplicable = t.PorcentajeAplicable,
            MontoBaseExceso     = t.MontoBaseExceso,
            CuotaFija           = t.CuotaFija,
            AnioVigencia        = t.AnioVigencia,
            Activo              = t.Activo
        }).ToList();
    }
}
