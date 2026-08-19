using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Commands.EliminarDiaFeriado;

public class EliminarDiaFeriadoCommandHandler : IRequestHandler<EliminarDiaFeriadoCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public EliminarDiaFeriadoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<bool> Handle(EliminarDiaFeriadoCommand request, CancellationToken cancellationToken)
    {
        var feriado = await _context.DiasFeriados
            .FirstOrDefaultAsync(f => f.IdDiaFeriado == request.IdDiaFeriado, cancellationToken);

        if (feriado is null)
            throw new KeyNotFoundException($"Día feriado con id {request.IdDiaFeriado} no encontrado.");

        _context.DiasFeriados.Remove(feriado);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
