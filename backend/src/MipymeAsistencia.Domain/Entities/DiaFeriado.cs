namespace MipymeAsistencia.Domain.Entities;

public class DiaFeriado
{
    public int IdDiaFeriado { get; set; }
    public DateTime Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsRecuperable { get; set; } = true;
    public bool EsMovil { get; set; } = false;
}
