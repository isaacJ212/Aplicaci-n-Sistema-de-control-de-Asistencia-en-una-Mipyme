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

        // ─── Validación de Estación de Trabajo por IP autorizada ───────────
        var sede = await _context.ConfiguracionesSede
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (sede != null && sede.ValidarIpEn2Fa && !string.IsNullOrWhiteSpace(request.IpOrigen))
        {
            var esPermitida = MipymeAsistencia.Application.Common.Helpers.IpAddressValidator.EsIpPermitida(
                request.IpOrigen, sede.IpEstacionPermitida);

            if (!esPermitida)
            {
                throw new UnauthorizedAccessException(
                    $"Acceso denegado: La estación de trabajo ({request.IpOrigen}) no está dentro del segmento de red autorizado por la sede.");
            }
        }

        // Registrar metadatos de validación exitosa
        if (!string.IsNullOrWhiteSpace(request.IpOrigen))
            usuario.UltimaIpLogin = request.IpOrigen.Trim();
        if (!string.IsNullOrWhiteSpace(request.MacAddress))
            usuario.UltimaMacLogin = request.MacAddress.Trim();
        usuario.UltimaFechaLogin = DateTime.UtcNow;

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
