namespace MipymeAsistencia.Domain.Entities;

public class PeriodoCierrePlanilla
{
    public int IdPeriodoCierre { get; set; }
    public string Periodo { get; set; } = string.Empty; // Formato: YYYY-MM
    public DateTime FechaCorteHorasExtras { get; set; }
    public DateTime FechaEmisionPlanilla { get; set; }
    public bool Cerrado { get; set; } = false;
    public DateTime? FechaCierreDefinitivo { get; set; }
    public int? IdUsuarioCierre { get; set; }
    public string? Observaciones { get; set; }

    public Usuario? UsuarioCierre { get; set; }
}
