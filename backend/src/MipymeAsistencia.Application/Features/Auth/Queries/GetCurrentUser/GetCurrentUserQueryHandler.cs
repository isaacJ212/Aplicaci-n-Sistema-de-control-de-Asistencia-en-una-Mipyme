using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.EstadoActivo, cancellationToken);

        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        // Busca el expediente de empleado asociado al usuario (puede no existir para usuarios Admin)
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdUsuario == usuario.IdUsuario, cancellationToken);

        return new CurrentUserDto
        {
            IdUsuario     = usuario.IdUsuario,
            IdEmpleado    = empleado?.IdEmpleado,
            Email         = usuario.Email,
            Role          = usuario.Rol?.NombreRol ?? "Empleado",
            EstadoActivo  = usuario.EstadoActivo,
            Es2FaActivo   = usuario.Es2FaActivo,
            FechaCreacion = usuario.FechaCreacion
        };
    }
}
