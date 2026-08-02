namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class AsistenciaResponseDto
{
    public int IdAsistencia { get; set; }
    public int IdEmpleado { get; set; }
    public DateTime Fecha { get; set; }
    public string? HoraEntrada { get; set; }
    public string? InicioAlmuerzo { get; set; }
    public string? FinAlmuerzo { get; set; }
    public string? HoraSalida { get; set; }
    public decimal LatitudMarcaje { get; set; }
    public decimal LongitudMarcaje { get; set; }
    public decimal DistanciaCalculadaMetros { get; set; }
    public string EstadoAsistencia { get; set; } = string.Empty;
    public int MinutosTardanza { get; set; }
    public bool EstaDentroDelRangoGps { get; set; }
    public string? Mensaje { get; set; }
}
