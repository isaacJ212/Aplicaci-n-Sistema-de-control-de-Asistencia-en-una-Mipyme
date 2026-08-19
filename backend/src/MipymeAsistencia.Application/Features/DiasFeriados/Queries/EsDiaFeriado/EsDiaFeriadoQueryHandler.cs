using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.EsDiaFeriado;

public class EsDiaFeriadoQueryHandler : IRequestHandler<EsDiaFeriadoQuery, DiaFeriadoDto?>
{
    private readonly IApplicationDbContext _context;

    public EsDiaFeriadoQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DiaFeriadoDto?> Handle(EsDiaFeriadoQuery request, CancellationToken cancellationToken)
    {
        var fechaBuscada = request.Fecha.Date;

        var f = await _context.DiasFeriados
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Fecha.Date == fechaBuscada, cancellationToken);

        if (f is null) return null;

        return new DiaFeriadoDto
        {
            IdDiaFeriado = f.IdDiaFeriado,
            Fecha        = f.Fecha,
            FechaFormato = f.Fecha.ToString("yyyy-MM-dd"),
            Nombre       = f.Nombre,
            Descripcion  = f.Descripcion,
            EsRecuperable = f.EsRecuperable,
            EsMovil      = f.EsMovil
        };
    }
}
