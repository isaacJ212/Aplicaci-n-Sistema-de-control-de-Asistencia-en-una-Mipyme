namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class RegistrarAsistenciaRequestDto
{
    public int IdEmpleado { get; set; }
    public string TipoMarcaje { get; set; } = "Entrada";
    public decimal LatitudMarcaje { get; set; }
    public decimal LongitudMarcaje { get; set; }
    public string TokenQrEscaneado { get; set; } = string.Empty;
    public string CodigoOtpGenerado { get; set; } = string.Empty;
}
