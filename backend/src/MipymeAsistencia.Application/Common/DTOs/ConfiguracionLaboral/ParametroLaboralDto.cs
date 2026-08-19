namespace MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

public class ParametroLaboralDto
{
    public int IdParametro { get; set; }
    public string Clave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; }
}
