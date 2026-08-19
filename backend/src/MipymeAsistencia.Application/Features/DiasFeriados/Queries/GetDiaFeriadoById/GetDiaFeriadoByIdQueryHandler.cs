using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiaFeriadoById;

public class GetDiaFeriadoByIdQueryHandler : IRequestHandler<GetDiaFeriadoByIdQuery, DiaFeriadoDto>
{
    private readonly IApplicationDbContext _context;

    public GetDiaFeriadoByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DiaFeriadoDto> Handle(GetDiaFeriadoByIdQuery request, CancellationToken cancellationToken)
    {
        var f = await _context.DiasFeriados
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdDiaFeriado == request.IdDiaFeriado, cancellationToken);

        if (f is null)
            throw new KeyNotFoundException($"Día feriado con id {request.IdDiaFeriado} no encontrado.");

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
