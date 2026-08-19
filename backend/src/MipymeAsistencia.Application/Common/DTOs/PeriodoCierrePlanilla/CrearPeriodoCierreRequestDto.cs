namespace MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

public class CrearPeriodoCierreRequestDto
{
    public string Periodo { get; set; } = string.Empty; // YYYY-MM
    public DateTime FechaCorteHorasExtras { get; set; }
    public DateTime FechaEmisionPlanilla { get; set; }
    public string? Observaciones { get; set; }
}
