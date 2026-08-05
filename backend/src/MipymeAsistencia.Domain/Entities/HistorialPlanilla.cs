namespace MipymeAsistencia.Domain.Entities;

public class HistorialPlanilla
{
    public int IdPlanilla { get; set; }
    public int IdEmpleado { get; set; }
    public string PeriodoMesAnio { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
    public decimal TotalHorasExtras { get; set; }
    public decimal PagoHorasExtras { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal InssLaboral { get; set; }
    public decimal IrLaboral { get; set; }
    public int MinutosTardanzaMes { get; set; }
    public decimal DeduccionTardanza { get; set; }
    public decimal Embargo { get; set; }
    public decimal Sindicato { get; set; }
    public decimal OtrasDeducciones { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal SalarioNeto { get; set; }
    public decimal AcumuladoAguinaldo { get; set; }
    public DateTime FechaEmision { get; set; }

    public Empleado? Empleado { get; set; }
}
