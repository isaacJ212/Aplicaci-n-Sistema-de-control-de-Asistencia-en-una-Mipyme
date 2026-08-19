using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.EliminarTablaIr;

public class EliminarTablaIrCommandHandler : IRequestHandler<EliminarTablaIrCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public EliminarTablaIrCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<bool> Handle(EliminarTablaIrCommand request, CancellationToken cancellationToken)
    {
        var tramo = await _context.TablaImpuestoRenta
            .FirstOrDefaultAsync(t => t.IdTablaIr == request.IdTablaIr, cancellationToken);

        if (tramo is null)
            throw new KeyNotFoundException($"Tramo de tabla IR con id {request.IdTablaIr} no encontrado.");

        _context.TablaImpuestoRenta.Remove(tramo);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
