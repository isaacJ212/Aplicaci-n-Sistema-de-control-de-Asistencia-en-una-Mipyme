namespace MipymeAsistencia.Application.Common.DTOs.Planilla;

/// <summary>
/// Payload para generar la planilla mensual de un empleado.
/// El sistema calcula automáticamente todas las deducciones y prestaciones
/// según la legislación laboral de Nicaragua (Ley 185 + Ley 822 LCT).
/// </summary>
public class GenerarPlanillaRequestDto
{
    public int     IdEmpleado       { get; set; }

    /// <summary>Formato YYYY-MM (ej. "2026-05")</summary>
    public string  PeriodoMesAnio   { get; set; } = string.Empty;

    /// <summary>Comisiones devengadas en el periodo (opcional, default 0).</summary>
    public decimal Comisiones       { get; set; } = 0m;

    /// <summary>Incentivos o bonificaciones adicionales (opcional, default 0).</summary>
    public decimal Incentivos       { get; set; } = 0m;

    /// <summary>Embargo judicial si aplica (opcional, default 0).</summary>
    public decimal Embargo          { get; set; } = 0m;

    /// <summary>Cuota sindical si aplica (opcional, default 0).</summary>
    public decimal Sindicato        { get; set; } = 0m;

    /// <summary>Otras deducciones adicionales (adelantos, préstamos, etc.).</summary>
    public decimal OtrasDeducciones { get; set; } = 0m;
}
