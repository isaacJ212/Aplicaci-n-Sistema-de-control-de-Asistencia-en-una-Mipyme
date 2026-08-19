namespace MipymeAsistencia.Application.Common.DTOs.Usuario;

public class UsuarioResponseDto
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Es2FaActivo { get; set; }
    public bool EstadoActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? UltimaIpLogin { get; set; }
    public string? UltimaMacLogin { get; set; }
    public DateTime? UltimaFechaLogin { get; set; }

    // Datos del empleado asociado (si existe)
    public int? IdEmpleado { get; set; }
    public string? CedulaIdentificacion { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? NombreCompleto { get; set; }
    public string? CargoFuncion { get; set; }
    public string? EstadoEmpleado { get; set; }
    public string? FotoUrl { get; set; }
}
