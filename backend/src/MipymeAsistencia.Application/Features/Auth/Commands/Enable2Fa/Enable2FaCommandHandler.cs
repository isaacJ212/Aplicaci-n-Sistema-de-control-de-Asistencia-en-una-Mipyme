using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Enable2Fa;

public class Enable2FaCommandHandler : IRequestHandler<Enable2FaCommand, object>
{
    private readonly IApplicationDbContext _context;

    public Enable2FaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(Enable2FaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.EstadoActivo, cancellationToken);

        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        usuario.Es2FaActivo = request.Enabled;
        if (!request.Enabled)
            usuario.Secret2Fa = null;

        await _context.SaveChangesAsync(cancellationToken);

        return new { enabled = usuario.Es2FaActivo };
    }
}
