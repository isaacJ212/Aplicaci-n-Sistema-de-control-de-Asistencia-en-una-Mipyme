namespace MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

public class PeriodoCierreDto
{
    public int IdPeriodoCierre { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaCorteHorasExtras { get; set; }
    public DateTime FechaEmisionPlanilla { get; set; }
    public bool Cerrado { get; set; }
    public DateTime? FechaCierreDefinitivo { get; set; }
    public int? IdUsuarioCierre { get; set; }
    public string? EmailUsuarioCierre { get; set; }
    public string? Observaciones { get; set; }
}
