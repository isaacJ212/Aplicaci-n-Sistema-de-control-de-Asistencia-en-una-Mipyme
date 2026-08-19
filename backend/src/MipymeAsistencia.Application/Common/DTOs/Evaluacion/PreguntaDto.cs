namespace MipymeAsistencia.Application.Common.DTOs.Evaluacion;

public class PreguntaDto
{
    public int    Numero    { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Texto     { get; set; } = string.Empty;
    public string Tipo      { get; set; } = string.Empty;
    public decimal Peso     { get; set; }
}
