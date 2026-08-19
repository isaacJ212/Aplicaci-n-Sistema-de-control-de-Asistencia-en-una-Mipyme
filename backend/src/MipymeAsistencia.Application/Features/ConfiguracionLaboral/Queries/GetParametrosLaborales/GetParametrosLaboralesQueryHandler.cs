using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetParametrosLaborales;

public class GetParametrosLaboralesQueryHandler : IRequestHandler<GetParametrosLaboralesQuery, List<ParametroLaboralDto>>
{
    private readonly IApplicationDbContext _context;

    public GetParametrosLaboralesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<ParametroLaboralDto>> Handle(GetParametrosLaboralesQuery request, CancellationToken cancellationToken)
    {
        var parametros = await _context.ParametrosLaborales
            .AsNoTracking()
            .OrderBy(p => p.IdParametro)
            .ToListAsync(cancellationToken);

        return parametros.Select(p => new ParametroLaboralDto
        {
            IdParametro       = p.IdParametro,
            Clave             = p.Clave,
            Valor             = p.Valor,
            Descripcion       = p.Descripcion,
            FechaModificacion = p.FechaModificacion
        }).ToList();
    }
}
