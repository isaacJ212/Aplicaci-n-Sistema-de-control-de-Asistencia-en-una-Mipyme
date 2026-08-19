namespace MipymeAsistencia.Application.Common.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? IdEmpleado { get; set; }
    public bool Requires2Fa { get; set; }
    public string? Message { get; set; }
    public bool Es2FaActivo { get; set; }
}
