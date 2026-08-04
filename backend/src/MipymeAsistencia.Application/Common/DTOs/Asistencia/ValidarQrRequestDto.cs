namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class ValidarQrRequestDto
{
    public int IdEmpleado { get; set; }
    public string TokenQrEscaneado { get; set; } = string.Empty;
}
