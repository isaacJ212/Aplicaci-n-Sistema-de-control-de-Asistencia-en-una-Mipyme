namespace MipymeAsistencia.Application.Common.DTOs.Evaluacion;

public class ResponderEvaluacionRequestDto
{
    /// <summary>
    /// Lista de 20 respuestas, una por pregunta.
    /// Calificación Likert 1-5.
    /// </summary>
    public List<RespuestaDto> Respuestas  { get; set; } = [];
    public string?            Observaciones { get; set; }
}
