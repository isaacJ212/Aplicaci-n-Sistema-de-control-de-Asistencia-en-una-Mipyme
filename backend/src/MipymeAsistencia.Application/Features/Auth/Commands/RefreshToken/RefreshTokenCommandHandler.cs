using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using RefreshTokenEntity = MipymeAsistencia.Domain.Entities.RefreshToken;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Busca el token en BD junto con el usuario y su rol
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.Usuario)
                .ThenInclude(u => u!.Rol)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.EsActivo)
            throw new UnauthorizedAccessException("El refresh token no es válido o ha expirado.");

        var usuario = storedToken.Usuario!;

        if (!usuario.EstadoActivo)
            throw new UnauthorizedAccessException("La cuenta de usuario está desactivada.");

        // Marca el token actual como utilizado (rotación de tokens)
        storedToken.FueUtilizado = true;

        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdUsuario == usuario.IdUsuario, cancellationToken);

        var rolNombre  = usuario.Rol?.NombreRol ?? "Empleado";
        var nuevoJwt   = _tokenService.GenerateToken(usuario.Email, rolNombre, usuario.IdUsuario, empleado?.IdEmpleado);
        var expiracion = DateTime.UtcNow.AddMinutes(120);

        var nuevoRefreshTokenValor = _tokenService.GenerateRefreshToken();
        var nuevoRefreshToken = new RefreshTokenEntity
        {
            IdUsuario       = usuario.IdUsuario,
            Token           = nuevoRefreshTokenValor,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion   = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(nuevoRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token        = nuevoJwt,
            RefreshToken = nuevoRefreshTokenValor,
            Expiration   = expiracion,
            Email        = usuario.Email,
            Role         = rolNombre,
            IdEmpleado   = empleado?.IdEmpleado
        };
    }
}
