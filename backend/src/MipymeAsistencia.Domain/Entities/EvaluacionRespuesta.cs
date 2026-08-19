namespace MipymeAsistencia.Domain.Entities;

/// <summary>
/// Respuesta individual a una de las 20 preguntas del formulario 360°.
/// Calificación escala Likert 1-5.
/// </summary>
public class EvaluacionRespuesta
{
    public int  IdRespuesta   { get; set; }
    public int  IdEvaluacion  { get; set; }

    /// <summary>Número de pregunta del 1 al 20 (corresponde a la tabla del FALTANTE.md).</summary>
    public int  NumeroPregunta { get; set; }

    /// <summary>Calificación Likert: 1 Malo · 2 Regular · 3 Bueno · 4 Muy bueno · 5 Excelente</summary>
    public int  Calificacion   { get; set; }

    // Navegación
    public EvaluacionDesempeno? Evaluacion { get; set; }
}
