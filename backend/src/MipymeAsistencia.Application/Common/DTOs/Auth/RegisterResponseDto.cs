namespace MipymeAsistencia.Application.Common.DTOs.Auth;

/// <summary>
/// Datos del usuario recién registrado que se devuelven al cliente.
/// No incluye PasswordHash ni ningún campo sensible.
/// </summary>
public class RegisterResponseDto
{
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EstadoActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
