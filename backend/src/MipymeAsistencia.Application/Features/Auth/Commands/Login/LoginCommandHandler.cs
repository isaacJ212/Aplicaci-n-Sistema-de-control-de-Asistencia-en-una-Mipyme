using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using RefreshTokenEntity = MipymeAsistencia.Domain.Entities.RefreshToken;
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

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var rolNombre  = usuario.Rol?.NombreRol ?? "Empleado";
        var jwt        = _tokenService.GenerateToken(usuario.Email, rolNombre);
        var expiracion = DateTime.UtcNow.AddMinutes(120);

        // Genera y persiste el Refresh Token en BD (7 días de vida)
        var refreshTokenValor = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshTokenEntity
        {
            IdUsuario       = usuario.IdUsuario,
            Token           = refreshTokenValor,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion   = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token        = jwt,
            RefreshToken = refreshTokenValor,
            Expiration   = expiracion,
            Email        = usuario.Email,
            Role         = rolNombre
        };
    }
}
