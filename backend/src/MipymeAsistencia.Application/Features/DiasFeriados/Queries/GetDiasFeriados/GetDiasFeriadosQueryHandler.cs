using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiasFeriados;

public class GetDiasFeriadosQueryHandler : IRequestHandler<GetDiasFeriadosQuery, List<DiaFeriadoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDiasFeriadosQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<DiaFeriadoDto>> Handle(GetDiasFeriadosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DiasFeriados.AsNoTracking();

        if (request.Anio.HasValue)
        {
            query = query.Where(f => f.Fecha.Year == request.Anio.Value);
        }

        var feriados = await query
            .OrderBy(f => f.Fecha)
            .ToListAsync(cancellationToken);

        return feriados.Select(f => new DiaFeriadoDto
        {
            IdDiaFeriado = f.IdDiaFeriado,
            Fecha        = f.Fecha,
            FechaFormato = f.Fecha.ToString("yyyy-MM-dd"),
            Nombre       = f.Nombre,
            Descripcion  = f.Descripcion,
            EsRecuperable = f.EsRecuperable,
            EsMovil      = f.EsMovil
        }).ToList();
    }
}
