namespace MipymeAsistencia.Application.Common.DTOs.Planilla;

/// <summary>
/// Planilla mensual calculada según la legislación nicaragüense.
/// Refleja exactamente la estructura de la planilla oficial Rubí del Valle.
///
/// INGRESOS:
///   Total Ingresos = Salario Básico + Comisiones + Horas Extras + Incentivos
///
/// DEDUCCIONES LABORALES (cargo al empleado):
///   INSS Laboral  = Total Ingresos * 7%  (Ley 539)
///   IR Laboral    = Proyección anual → tabla progresiva LCT Ley 822 → /12
///   Total Deducciones = INSS + IR + Embargo + Sindicato + OtrasDeducciones
///
/// APORTES PATRONALES (cargo a la empresa, informativo):
///   INSS Patronal = Total Ingresos * 21.5%
///   INATEC        = Total Ingresos * 2%
///
/// PRESTACIONES SOCIALES (provisión mensual):
///   Vacaciones     = (Salario Básico / 30) * 2.5
///   Aguinaldo      = (Salario Básico / 30) * 2.5
///   Indemnización  = (Salario Básico / 30) * 2.5
/// </summary>
public class PlanillaResponseDto
{
    public int      IdPlanilla              { get; set; }
    public int      IdEmpleado              { get; set; }
    public string   NombreEmpleado          { get; set; } = string.Empty;
    public string   CargoEmpleado           { get; set; } = string.Empty;
    public string   PeriodoMesAnio          { get; set; } = string.Empty;

    // ── Ingresos ─────────────────────────────────────────────────────────────
    public decimal  SalarioBase             { get; set; }
    public decimal  Comisiones              { get; set; }
    public decimal  TotalHorasExtras        { get; set; }   // horas (cantidad)
    public decimal  PagoHorasExtras         { get; set; }   // monto C$
    public decimal  Incentivos              { get; set; }
    public decimal  TotalIngresos           { get; set; }

    // ── Deducciones laborales ────────────────────────────────────────────────
    public decimal  InssLaboral             { get; set; }   // 7%
    public decimal  IrLaboral               { get; set; }   // tabla progresiva
    public decimal  Embargo                 { get; set; }
    public decimal  Sindicato               { get; set; }
    public decimal  OtrasDeducciones        { get; set; }
    public decimal  TotalDeducciones        { get; set; }

    // ── Neto ─────────────────────────────────────────────────────────────────
    public decimal  SalarioNeto             { get; set; }

    // ── Aportes patronales (informativo, no descuenta al empleado) ───────────
    public decimal  InssPatronal            { get; set; }   // 21.5%
    public decimal  Inatec                  { get; set; }   // 2%

    // ── Provisión de prestaciones sociales mensuales ─────────────────────────
    public decimal  AcumuladoVacaciones     { get; set; }   // SalBase/30*2.5
    public decimal  AcumuladoAguinaldo      { get; set; }   // SalBase/30*2.5
    public decimal  AcumuladoIndemnizacion  { get; set; }   // SalBase/30*2.5

    public DateTime FechaEmision            { get; set; }
}
