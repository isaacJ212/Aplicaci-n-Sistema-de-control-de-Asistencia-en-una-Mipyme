using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Verify2Fa;

public class Verify2FaCommandHandler : IRequestHandler<Verify2FaCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService        _tokenService;
    private readonly ICodigo2FaService    _codigo2FaService;

    public Verify2FaCommandHandler(
        IApplicationDbContext context,
        ITokenService        tokenService,
        ICodigo2FaService    codigo2FaService)
    {
        _context          = context;
        _tokenService     = tokenService;
        _codigo2FaService = codigo2FaService;
    }

    public async Task<LoginResponseDto> Handle(Verify2FaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.EstadoActivo, cancellationToken);

        if (usuario is null)
            throw new UnauthorizedAccessException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(usuario.Secret2Fa) || !BCrypt.Net.BCrypt.Verify(request.Code, usuario.Secret2Fa))
            throw new UnauthorizedAccessException("Código de verificación inválido.");

        // Invalida el código en cache después de usarlo correctamente (un solo uso)
        _codigo2FaService.Invalidar(usuario.Email);

        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdUsuario == usuario.IdUsuario, cancellationToken);

        var rolNombre = usuario.Rol?.NombreRol ?? "Empleado";
        var jwt = _tokenService.GenerateToken(usuario.Email, rolNombre, usuario.IdUsuario, empleado?.IdEmpleado);
        var refreshTokenValor = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new MipymeAsistencia.Domain.Entities.RefreshToken
        {
            IdUsuario = usuario.IdUsuario,
            Token = refreshTokenValor,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Token = jwt,
            RefreshToken = refreshTokenValor,
            Expiration = DateTime.UtcNow.AddMinutes(120),
            Email = usuario.Email,
            Role = rolNombre,
            IdEmpleado = empleado?.IdEmpleado,
            Es2FaActivo = usuario.Es2FaActivo,
            Requires2Fa = false,
            Message = "Validación 2FA correcta."
        };
    }
}
