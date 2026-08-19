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

        var rolNombre = usuario.Rol?.NombreRol ?? "Empleado";
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdUsuario == usuario.IdUsuario, cancellationToken);

        if (usuario.Es2FaActivo)
        {
            var codigo = Random.Shared.Next(100000, 999999).ToString("D6");
            usuario.Secret2Fa = BCrypt.Net.BCrypt.HashPassword(codigo);
            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResponseDto
            {
                Email = usuario.Email,
                Role = rolNombre,
                IdEmpleado = empleado?.IdEmpleado,
                Es2FaActivo = true,
                Requires2Fa = true,
                Message = "Se ha enviado un código de verificación a su estación de trabajo."
            };
        }

        var jwt = _tokenService.GenerateToken(usuario.Email, rolNombre, usuario.IdUsuario, empleado?.IdEmpleado);
        var expiracion = DateTime.UtcNow.AddMinutes(120);

        var refreshTokenValor = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshTokenEntity
        {
            IdUsuario = usuario.IdUsuario,
            Token = refreshTokenValor,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = jwt,
            RefreshToken = refreshTokenValor,
            Expiration = expiracion,
            Email = usuario.Email,
            Role = rolNombre,
            IdEmpleado = empleado?.IdEmpleado,
            Es2FaActivo = usuario.Es2FaActivo,
            Requires2Fa = false,
            Message = "Inicio de sesión exitoso."
        };
    }
}
