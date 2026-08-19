namespace MipymeAsistencia.Application.Common.DTOs.Empleado;

public class AcumularVacacionesResponseDto
{
    public int     IdEmpleado                  { get; set; }
    public string  NombreEmpleado              { get; set; } = string.Empty;
    public DateTime FechaContratacion          { get; set; }
    public int     MesesTrabajados             { get; set; }
    public int     DiasTrabajadosReales        { get; set; }
    /// <summary>Días acumulados teóricos (2.5 × meses completos).</summary>
    public decimal DiasAcumuladosTeoricos      { get; set; }
    /// <summary>Días ya descontados por vacaciones aprobadas.</summary>
    public decimal DiasDescontadosVacaciones   { get; set; }
    /// <summary>Saldo final actualizado en la BD.</summary>
    public decimal DiasVacacionesDisponibles   { get; set; }
}
