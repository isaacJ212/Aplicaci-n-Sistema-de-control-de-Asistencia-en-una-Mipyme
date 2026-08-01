namespace MipymeAsistencia.Domain.Entities;

public class EvaluacionDesempeno
{
    public int IdEvaluacion { get; set; }
    public int IdEmpleado { get; set; }
    public int IdEvaluador { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal? PorcentajePuntualidad { get; set; }
    public int? CalificacionCumplimientoFunciones { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaEvaluacion { get; set; } = DateTime.UtcNow;

    public Empleado? Empleado { get; set; }
    public Usuario? Evaluador { get; set; }
}
