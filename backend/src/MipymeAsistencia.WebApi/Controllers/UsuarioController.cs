using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Usuario;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuarioController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UsuarioController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos los usuarios con su rol y expediente de empleado vinculado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? estadoActivo = null,
        [FromQuery] int? idRol = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .AsNoTracking()
            .AsQueryable();

        if (estadoActivo.HasValue)
        {
            query = query.Where(u => u.EstadoActivo == estadoActivo.Value);
        }

        if (idRol.HasValue && idRol.Value > 0)
        {
            query = query.Where(u => u.IdRol == idRol.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(s) ||
                (u.Empleado != null && (
                    u.Empleado.Nombres.ToLower().Contains(s) ||
                    u.Empleado.Apellidos.ToLower().Contains(s) ||
                    u.Empleado.CedulaIdentificacion.ToLower().Contains(s) ||
                    u.Empleado.CargoFuncion.ToLower().Contains(s)
                ))
            );
        }

        var usuarios = await query
            .OrderBy(u => u.IdUsuario)
            .Select(u => new UsuarioResponseDto
            {
                IdUsuario            = u.IdUsuario,
                IdRol                = u.IdRol,
                NombreRol            = u.Rol != null ? u.Rol.NombreRol : "Sin Rol",
                Email                = u.Email,
                Es2FaActivo          = u.Es2FaActivo,
                EstadoActivo         = u.EstadoActivo,
                FechaCreacion        = u.FechaCreacion,
                UltimaIpLogin        = u.UltimaIpLogin,
                UltimaMacLogin       = u.UltimaMacLogin,
                UltimaFechaLogin     = u.UltimaFechaLogin,
                IdEmpleado           = u.Empleado != null ? u.Empleado.IdEmpleado : null,
                CedulaIdentificacion = u.Empleado != null ? u.Empleado.CedulaIdentificacion : null,
                Nombres              = u.Empleado != null ? u.Empleado.Nombres : null,
                Apellidos            = u.Empleado != null ? u.Empleado.Apellidos : null,
                NombreCompleto       = u.Empleado != null ? $"{u.Empleado.Nombres} {u.Empleado.Apellidos}".Trim() : null,
                CargoFuncion         = u.Empleado != null ? u.Empleado.CargoFuncion : null,
                EstadoEmpleado       = u.Empleado != null ? u.Empleado.EstadoEmpleado : null,
                FotoUrl              = u.Empleado != null ? u.Empleado.FotoUrl : null,
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<UsuarioResponseDto>>.Ok(
            usuarios, $"Se obtuvieron {usuarios.Count} usuario(s) correctamente."));
    }

    /// <summary>
    /// Obtiene los detalles de un usuario específico.
    /// </summary>
    [HttpGet("{idUsuario:int}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int idUsuario, CancellationToken ct = default)
    {
        var u = await _context.Usuarios
            .Include(x => x.Rol)
            .Include(x => x.Empleado)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario, ct);

        if (u is null)
            return NotFound(ApiResponse<object>.NotFound($"Usuario con ID {idUsuario} no encontrado."));

        var dto = new UsuarioResponseDto
        {
            IdUsuario            = u.IdUsuario,
            IdRol                = u.IdRol,
            NombreRol            = u.Rol != null ? u.Rol.NombreRol : "Sin Rol",
            Email                = u.Email,
            Es2FaActivo          = u.Es2FaActivo,
            EstadoActivo         = u.EstadoActivo,
            FechaCreacion        = u.FechaCreacion,
            UltimaIpLogin        = u.UltimaIpLogin,
            UltimaMacLogin       = u.UltimaMacLogin,
            UltimaFechaLogin     = u.UltimaFechaLogin,
            IdEmpleado           = u.Empleado != null ? u.Empleado.IdEmpleado : null,
            CedulaIdentificacion = u.Empleado != null ? u.Empleado.CedulaIdentificacion : null,
            Nombres              = u.Empleado != null ? u.Empleado.Nombres : null,
            Apellidos            = u.Empleado != null ? u.Empleado.Apellidos : null,
            NombreCompleto       = u.Empleado != null ? $"{u.Empleado.Nombres} {u.Empleado.Apellidos}".Trim() : null,
            CargoFuncion         = u.Empleado != null ? u.Empleado.CargoFuncion : null,
            EstadoEmpleado       = u.Empleado != null ? u.Empleado.EstadoEmpleado : null,
            FotoUrl              = u.Empleado != null ? u.Empleado.FotoUrl : null,
        };

        return Ok(ApiResponse<UsuarioResponseDto>.Ok(dto, "Usuario obtenido correctamente."));
    }

    /// <summary>
    /// Cambia el estado (Activo / Inactivo) de un usuario.
    /// </summary>
    [HttpPut("{idUsuario:int}/estado")]
    [HttpPatch("{idUsuario:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        int idUsuario,
        [FromBody] CambiarEstadoUsuarioDto request,
        CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null)
            return NotFound(ApiResponse<object>.NotFound($"Usuario con ID {idUsuario} no encontrado."));

        // Validar no auto-desactivar al admin logueado si es el único
        var emailActual = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("sub");
        if (usuario.Email == emailActual && !request.EstadoActivo)
        {
            return BadRequest(ApiResponse<object>.BadRequest(
                "No puedes desactivar tu propia cuenta mientras estás en sesión."));
        }

        usuario.EstadoActivo = request.EstadoActivo;

        // Si se desactiva el usuario y tiene empleado asociado, opcionalmente sincronizar
        await _context.SaveChangesAsync(ct);

        var dto = new UsuarioResponseDto
        {
            IdUsuario            = usuario.IdUsuario,
            IdRol                = usuario.IdRol,
            NombreRol            = usuario.Rol != null ? usuario.Rol.NombreRol : "Sin Rol",
            Email                = usuario.Email,
            Es2FaActivo          = usuario.Es2FaActivo,
            EstadoActivo         = usuario.EstadoActivo,
            FechaCreacion        = usuario.FechaCreacion,
            UltimaIpLogin        = usuario.UltimaIpLogin,
            UltimaMacLogin       = usuario.UltimaMacLogin,
            UltimaFechaLogin     = usuario.UltimaFechaLogin,
            IdEmpleado           = usuario.Empleado?.IdEmpleado,
            NombreCompleto       = usuario.Empleado != null ? $"{usuario.Empleado.Nombres} {usuario.Empleado.Apellidos}".Trim() : null,
            EstadoEmpleado       = usuario.Empleado?.EstadoEmpleado,
        };

        var estadoTexto = request.EstadoActivo ? "activado" : "desactivado";
        return Ok(ApiResponse<UsuarioResponseDto>.Ok(
            dto, $"Usuario '{usuario.Email}' ha sido {estadoTexto} correctamente."));
    }

    /// <summary>
    /// Cambia el rol de un usuario (Admin, Analista, Empleado).
    /// </summary>
    [HttpPut("{idUsuario:int}/rol")]
    [HttpPatch("{idUsuario:int}/rol")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(
        int idUsuario,
        [FromBody] CambiarRolUsuarioDto request,
        CancellationToken ct = default)
    {
        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.IdRol == request.IdRol, ct);
        if (rol is null)
            return BadRequest(ApiResponse<object>.BadRequest($"El rol con ID {request.IdRol} no existe."));

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null)
            return NotFound(ApiResponse<object>.NotFound($"Usuario con ID {idUsuario} no encontrado."));

        usuario.IdRol = request.IdRol;
        usuario.Rol   = rol;
        await _context.SaveChangesAsync(ct);

        var dto = new UsuarioResponseDto
        {
            IdUsuario            = usuario.IdUsuario,
            IdRol                = usuario.IdRol,
            NombreRol            = rol.NombreRol,
            Email                = usuario.Email,
            Es2FaActivo          = usuario.Es2FaActivo,
            EstadoActivo         = usuario.EstadoActivo,
            FechaCreacion        = usuario.FechaCreacion,
            IdEmpleado           = usuario.Empleado?.IdEmpleado,
            NombreCompleto       = usuario.Empleado != null ? $"{usuario.Empleado.Nombres} {usuario.Empleado.Apellidos}".Trim() : null,
        };

        return Ok(ApiResponse<UsuarioResponseDto>.Ok(
            dto, $"Rol del usuario '{usuario.Email}' actualizado a '{rol.NombreRol}'."));
    }

    /// <summary>
    /// Restablece la contraseña de un usuario como administrador.
    /// </summary>
    [HttpPut("{idUsuario:int}/reset-password")]
    [HttpPost("{idUsuario:int}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        int idUsuario,
        [FromBody] ResetPasswordUsuarioDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NuevaPassword) || request.NuevaPassword.Length < 6)
            return BadRequest(ApiResponse<object>.BadRequest(
                "La nueva contraseña debe tener al menos 6 caracteres."));

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);
        if (usuario is null)
            return NotFound(ApiResponse<object>.NotFound($"Usuario con ID {idUsuario} no encontrado."));

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(
            null!, $"Contraseña del usuario '{usuario.Email}' restablecida correctamente."));
    }

    /// <summary>
    /// Lista todos los roles disponibles en el sistema.
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<List<RolDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.IdRol)
            .Select(r => new RolDto
            {
                IdRol       = r.IdRol,
                NombreRol   = r.NombreRol,
                Descripcion = r.Descripcion,
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<RolDto>>.Ok(roles, "Lista de roles obtenida correctamente."));
    }

    /// <summary>
    /// Crea un nuevo usuario directamente desde el panel de administración.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearUsuario(
        [FromBody] CrearUsuarioRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return BadRequest(ApiResponse<object>.BadRequest("El formato del correo electrónico es inválido."));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest(ApiResponse<object>.BadRequest("La contraseña debe tener al menos 6 caracteres."));

        var existe = await _context.Usuarios.AnyAsync(u => u.Email == request.Email.Trim().ToLower(), ct);
        if (existe)
            return Conflict(ApiResponse<object>.Conflict("Ya existe un usuario con este correo electrónico."));

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.IdRol == request.IdRol, ct);
        if (rol is null)
            return BadRequest(ApiResponse<object>.BadRequest("El rol especificado no existe."));

        var usuario = new Usuario
        {
            Email         = request.Email.Trim().ToLower(),
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IdRol         = request.IdRol,
            EstadoActivo  = true,
            Es2FaActivo   = false,
            FechaCreacion = DateTime.UtcNow,
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);

        var dto = new UsuarioResponseDto
        {
            IdUsuario     = usuario.IdUsuario,
            IdRol         = usuario.IdRol,
            NombreRol     = rol.NombreRol,
            Email         = usuario.Email,
            Es2FaActivo   = false,
            EstadoActivo  = true,
            FechaCreacion = usuario.FechaCreacion,
        };

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<UsuarioResponseDto>.Created(dto, "Usuario creado exitosamente."));
    }
}
