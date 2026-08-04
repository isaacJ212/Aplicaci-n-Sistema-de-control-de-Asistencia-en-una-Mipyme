namespace MipymeAsistencia.Application.Common.DTOs.HoraExtra;

/// <summary>
/// Datos de una hora extra devueltos al cliente.
/// El monto se calcula según Arto. 62 Ley 185 Nicaragua:
/// (SalarioMensual / 240) * Factor * CantidadHoras
/// Factor 1.5 turno diurno normal, 1.8 turno mixto/nocturno o días de descanso.
/// </summary>
public class HoraExtraResponseDto
{
    public int      IdHoraExtra        { get; set; }
    public int      IdEmpleado         { get; set; }
    public string   NombreEmpleado     { get; set; } = string.Empty;
    public int?     IdUsuarioAprobador { get; set; }
    public string?  NombreAprobador    { get; set; }
    public DateTime Fecha              { get; set; }
    public decimal  CantidadHoras      { get; set; }
    public string   Motivo             { get; set; } = string.Empty;

    /// <summary>Calculado: (SalarioMensual/240) * Factor * CantidadHoras</summary>
    public decimal  MontoPagar         { get; set; }

    /// <summary>Pendiente | Aprobado | Rechazado</summary>
    public string   Estado             { get; set; } = string.Empty;
}
