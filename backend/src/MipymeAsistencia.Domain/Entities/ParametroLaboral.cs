namespace MipymeAsistencia.Domain.Entities;

public class ParametroLaboral
{
    public int IdParametro { get; set; }
    public string Clave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
}
