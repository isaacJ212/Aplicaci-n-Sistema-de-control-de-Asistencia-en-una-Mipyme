namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class QrActualResponseDto
{
    public int IdSede { get; set; }
    public string NombreSede { get; set; } = string.Empty;
    public string TokenQrActual { get; set; } = string.Empty;
    public DateTime? QrUltimaActualizacion { get; set; }
    public int RadioToleranciaMetros { get; set; }
}
