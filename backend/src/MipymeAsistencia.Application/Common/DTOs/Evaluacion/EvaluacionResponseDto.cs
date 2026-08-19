namespace MipymeAsistencia.Application.Common.DTOs.Evaluacion;

public class EvaluacionResponseDto
{
    public int     IdEvaluacion    { get; set; }
    public int     IdEmpleado      { get; set; }
    public string  NombreEmpleado  { get; set; } = string.Empty;
    public int     IdEvaluador     { get; set; }
    public string  NombreEvaluador { get; set; } = string.Empty;
    public string  Perspectiva     { get; set; } = string.Empty;
    public string  Periodo         { get; set; } = string.Empty;
    public decimal PuntajeFinal    { get; set; }
    public string? Observaciones   { get; set; }
    public string  Estado          { get; set; } = string.Empty;
    public DateTime  FechaCreacion    { get; set; }
    public DateTime? FechaCompletada  { get; set; }
    /// <summary>Solo se incluye en GetById, vacío en listas.</summary>
    public List<RespuestaDto> Respuestas { get; set; } = [];
}
