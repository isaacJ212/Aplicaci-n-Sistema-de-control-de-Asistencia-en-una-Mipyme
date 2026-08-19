namespace MipymeAsistencia.Domain.Entities;

/// <summary>
/// Cabecera de una evaluación 360°.
/// Una evaluación agrupa las respuestas de UN evaluador sobre UN evaluado
/// para un período semestral específico.
/// </summary>
public class EvaluacionDesempeno
{
    public int     IdEvaluacion   { get; set; }

    /// <summary>Empleado que es evaluado.</summary>
    public int     IdEmpleado     { get; set; }

    /// <summary>Usuario que realiza la evaluación (puede ser el mismo empleado → autoevaluación).</summary>
    public int     IdEvaluador    { get; set; }

    /// <summary>Perspectiva: Autoevaluacion | Jefe | Par | Subordinado</summary>
    public string  Perspectiva    { get; set; } = "Jefe";

    /// <summary>Período semestral, ej. "2026-S1", "2026-S2"</summary>
    public string  Periodo        { get; set; } = string.Empty;

    /// <summary>Puntaje final calculado con la fórmula ponderada (0-100).</summary>
    public decimal PuntajeFinal   { get; set; } = 0m;

    public string? Observaciones  { get; set; }

    /// <summary>Estado: Pendiente | Completada</summary>
    public string  Estado         { get; set; } = "Pendiente";

    public DateTime FechaCreacion    { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCompletada { get; set; }

    // Navegación
    public Empleado? Empleado   { get; set; }
    public Usuario?  Evaluador  { get; set; }
    public ICollection<EvaluacionRespuesta> Respuestas { get; set; } = new List<EvaluacionRespuesta>();
}
