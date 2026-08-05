namespace MipymeAsistencia.Domain.Entities;

public class ConfiguracionSede
{
    public int IdSede { get; set; }
    public string NombreSede { get; set; } = "Sede Principal";
    public decimal LatitudSede { get; set; }
    public decimal LongitudSede { get; set; }
    public int RadioToleranciaMetros { get; set; } = 100;
    public TimeSpan HoraEntradaOficial { get; set; } = new(8, 0, 0);
    public TimeSpan HoraSalidaOficial { get; set; } = new(17, 0, 0);
    public int DuracionAlmuerzoMinutos { get; set; } = 60;
    public int MinutosTolerancia { get; set; } = 10;
    public string? TokenQrActual { get; set; }
    public DateTime? QrUltimaActualizacion { get; set; }
}
