namespace MipymeAsistencia.Domain.Entities;

public class HistorialAsistencia
{
    public int IdAsistencia { get; set; }
    public int IdEmpleado { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan HoraEntrada { get; set; }
    public TimeSpan? InicioAlmuerzo { get; set; }
    public TimeSpan? FinAlmuerzo { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public decimal LatitudMarcaje { get; set; }
    public decimal LongitudMarcaje { get; set; }
    public decimal DistanciaCalculadaMetros { get; set; }
    public string EstadoAsistencia { get; set; } = string.Empty;
    public int MinutosTardanza { get; set; }
    public bool EstaDentroDelRangoGps { get; set; } = true;

    public Empleado? Empleado { get; set; }
}
