namespace MipymeAsistencia.Application.Common.DTOs.Evaluacion;

public class AsignarMasivoRequestDto
{
    /// <summary>Período semestral, ej. "2026-S1", "2026-S2"</summary>
    public string Periodo     { get; set; } = string.Empty;

    /// <summary>Perspectiva inicial por defecto: Autoevaluacion | Jefe | Par | Subordinado</summary>
    public string Perspectiva { get; set; } = "Autoevaluacion";
}
