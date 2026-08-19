namespace MipymeAsistencia.Application.Common.DTOs.Auth;

public class Verify2FaRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IpOrigen { get; set; }
    public string? MacAddress { get; set; }
}
