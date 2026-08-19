namespace MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

public class TablaIrDto
{
    public int IdTablaIr { get; set; }
    public decimal DesdeMontoAnual { get; set; }
    public decimal? HastaMontoAnual { get; set; }
    public decimal PorcentajeAplicable { get; set; }
    public decimal PorcentajeVisual => PorcentajeAplicable * 100m;
    public decimal MontoBaseExceso { get; set; }
    public decimal CuotaFija { get; set; }
    public int AnioVigencia { get; set; }
    public bool Activo { get; set; }
}
