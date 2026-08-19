namespace MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

public class ActualizarTablaIrRequestDto
{
    public decimal DesdeMontoAnual { get; set; }
    public decimal? HastaMontoAnual { get; set; }
    public decimal PorcentajeAplicable { get; set; }
    public decimal MontoBaseExceso { get; set; }
    public decimal CuotaFija { get; set; }
    public int AnioVigencia { get; set; } = 2026;
    public bool Activo { get; set; } = true;
}
