namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class ResultadoSincronizacionDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int TotalDispositivosProcesados { get; set; }
    public int TotalMarcajesLeidos { get; set; }
    public int TotalMarcajesNuevos { get; set; }
    public int TotalAsistenciasGeneradas { get; set; }
    public int TotalErrores { get; set; }
    public List<string> Detalles { get; set; } = new();
    public DateTime FechaEjecucionUtc { get; set; } = DateTime.UtcNow;
}
