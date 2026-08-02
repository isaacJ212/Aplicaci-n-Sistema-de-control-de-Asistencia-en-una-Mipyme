using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExistente = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExistente)
            throw new InvalidOperationException("El email ya está registrado.");

        // Busca el rol solicitado; si no existe cae al rol "Empleado" por defecto
        var rol = await _context.Roles
                      .FirstOrDefaultAsync(r => r.NombreRol == request.Role, cancellationToken)
                  ?? await _context.Roles
                      .FirstOrDefaultAsync(r => r.NombreRol == "Empleado", cancellationToken);

        if (rol is null)
            throw new InvalidOperationException("No existe un rol válido para el usuario.");

        var usuario = new Usuario
        {
            Email       = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IdRol        = rol.IdRol,
            EstadoActivo = true,
            Es2FaActivo  = false,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        // Devuelve únicamente los campos limpios — sin PasswordHash ni datos internos
        return new RegisterResponseDto
        {
            IdUsuario    = usuario.IdUsuario,
            Email        = usuario.Email,
            Role         = rol.NombreRol,
            EstadoActivo = usuario.EstadoActivo,
            FechaCreacion = usuario.FechaCreacion
        };
    }
}
