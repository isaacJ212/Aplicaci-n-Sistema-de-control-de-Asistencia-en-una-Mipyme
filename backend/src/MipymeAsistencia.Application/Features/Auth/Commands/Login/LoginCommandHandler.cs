using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.EstadoActivo, cancellationToken);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var token = _tokenService.GenerateToken(usuario.Email, usuario.Rol?.NombreRol ?? "Empleado");

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = _tokenService.GenerateRefreshToken(),
            Expiration = DateTime.UtcNow.AddMinutes(120),
            Email = usuario.Email,
            Role = usuario.Rol?.NombreRol ?? "Empleado"
        };
    }
}
