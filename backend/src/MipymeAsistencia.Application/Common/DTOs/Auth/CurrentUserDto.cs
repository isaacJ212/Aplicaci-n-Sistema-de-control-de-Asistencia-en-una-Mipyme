namespace MipymeAsistencia.Application.Common.DTOs.Auth;

/// <summary>
/// Datos del usuario autenticado expuestos por el endpoint /auth/me.
/// No incluye datos sensibles.
/// </summary>
public class CurrentUserDto
{
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EstadoActivo { get; set; }
    public bool Es2FaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
