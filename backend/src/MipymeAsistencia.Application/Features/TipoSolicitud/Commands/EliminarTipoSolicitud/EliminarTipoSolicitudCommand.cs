using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.TipoSolicitud.Commands.EliminarTipoSolicitud;

public class EliminarTipoSolicitudCommand : IRequest<bool>
{
    public int IdTipoSolicitud { get; set; }
}

public class EliminarTipoSolicitudCommandHandler : IRequestHandler<EliminarTipoSolicitudCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public EliminarTipoSolicitudCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<bool> Handle(EliminarTipoSolicitudCommand request, CancellationToken cancellationToken)
    {
        var tipo = await _context.TiposSolicitudPermiso
            .FirstOrDefaultAsync(t => t.IdTipoSolicitud == request.IdTipoSolicitud, cancellationToken);

        if (tipo is null)
            throw new KeyNotFoundException($"Tipo de solicitud #{request.IdTipoSolicitud} no encontrado.");

        _context.TiposSolicitudPermiso.Remove(tipo);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
