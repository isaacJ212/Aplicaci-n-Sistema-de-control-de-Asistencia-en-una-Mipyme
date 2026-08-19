namespace MipymeAsistencia.Application.Common.DTOs.Evaluacion;

public class CrearEvaluacionRequestDto
{
    public int    IdEmpleado  { get; set; }
    /// <summary>
    /// IdUsuario del evaluador a asignar.
    /// Si es 0 o no se envía, el controller usa el IdUsuario del JWT.
    /// </summary>
    public int?   IdEvaluadorOverride { get; set; }
    /// <summary>Perspectiva: Autoevaluacion | Jefe | Par | Subordinado</summary>
    public string Perspectiva { get; set; } = "Jefe";
    /// <summary>Período semestral, ej. "2026-S1"</summary>
    public string Periodo     { get; set; } = string.Empty;
}
