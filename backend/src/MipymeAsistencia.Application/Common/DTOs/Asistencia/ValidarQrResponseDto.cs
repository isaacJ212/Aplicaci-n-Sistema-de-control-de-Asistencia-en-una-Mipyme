namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class ValidarQrResponseDto
{
    public bool EsValido { get; set; }
    public string CodigoOtpGenerado { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
