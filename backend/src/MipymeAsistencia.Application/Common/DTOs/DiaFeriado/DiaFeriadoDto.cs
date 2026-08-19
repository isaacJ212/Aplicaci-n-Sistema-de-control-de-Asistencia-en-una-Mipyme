namespace MipymeAsistencia.Application.Common.DTOs.DiaFeriado;

public class DiaFeriadoDto
{
    public int IdDiaFeriado { get; set; }
    public DateTime Fecha { get; set; }
    public string FechaFormato { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsRecuperable { get; set; }
    public bool EsMovil { get; set; }
}
