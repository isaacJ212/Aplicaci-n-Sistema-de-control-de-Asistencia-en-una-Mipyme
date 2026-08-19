using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Common.Interfaces;
using RefreshTokenEntity = MipymeAsistencia.Domain.Entities.RefreshToken;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext         _context;
    private readonly ITokenService                _tokenService;
    private readonly ICodigo2FaService            _codigo2FaService;
    private readonly INotificadorEstacionService? _notificador;

    public LoginCommandHandler(
        IApplicationDbContext context,
        ITokenService         tokenService,
        ICodigo2FaService     codigo2FaService,
        INotificadorEstacionService? notificador = null)
    {
        _context          = context;
        _tokenService     = tokenService;
        _codigo2FaService = codigo2FaService;
        _notificador      = notificador;
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
            var duracion = TimeSpan.FromMinutes(5);
            var expira   = DateTime.UtcNow.Add(duracion);

            // 1. Guardar hash en BD (para Verify2Fa)
            usuario.Secret2Fa = BCrypt.Net.BCrypt.HashPassword(codigo);
            await _context.SaveChangesAsync(cancellationToken);

            // 2. Guardar código plano temporalmente (5 min) para:
            //    - Entrega a estación vía SignalR
            //    - Endpoint fallback GET /api/auth/codigo-2fa
            _codigo2FaService.Guardar(usuario.Email, codigo, duracion);

            // 3. Notificar en tiempo real a la estación de trabajo (SignalR)
            //    Si SignalR no está registrado o falla, el fallback endpoint sigue funcionando.
            if (_notificador is not null)
            {
                try
                {
                    await _notificador.NotificarCodigo2FaAsync(usuario.Email, codigo, expira);
                }
                catch
                {
                    // Ignorar fallos de SignalR — el usuario aún puede obtener el
                    // código por el endpoint de consulta o por otro canal.
                }
            }

            return new LoginResponseDto
            {
                Email = usuario.Email,
                Role = rolNombre,
                IdEmpleado = empleado?.IdEmpleado,
                Es2FaActivo = true,
                Requires2Fa = true,
                Codigo2FaSoloPruebas = codigo,
                Expiration = expira,
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
