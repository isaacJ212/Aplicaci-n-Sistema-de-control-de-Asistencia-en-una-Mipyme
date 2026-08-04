namespace MipymeAsistencia.Application.Common.DTOs.HoraExtra;

/// <summary>
/// Payload para registrar horas extras de un empleado.
/// El monto se calcula automáticamente en el handler usando el salario del empleado.
/// Factor: 1.5 turno diurno, 1.8 turno nocturno/días de descanso (Arto. 62 Ley 185).
/// </summary>
public class RegistrarHoraExtraRequestDto
{
    public int      IdEmpleado    { get; set; }
    public DateTime Fecha         { get; set; }
    public decimal  CantidadHoras { get; set; }
    public string   Motivo        { get; set; } = string.Empty;

    /// <summary>
    /// Factor de recargo según Ley 185 Nicaragua:
    /// 1.5 = turno diurno ordinario
    /// 1.8 = turno nocturno, mixto o día de descanso/feriado
    /// </summary>
    public decimal FactorRecargo { get; set; } = 1.5m;
}
