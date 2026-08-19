namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

public class InformeAsistenciaDto
{
    public int     IdEmpleado            { get; set; }
    public string  NombreEmpleado        { get; set; } = string.Empty;
    public string  CargoFuncion          { get; set; } = string.Empty;
    public string? FotoUrl               { get; set; }

    // Período del informe
    public DateTime FechaDesde           { get; set; }
    public DateTime FechaHasta           { get; set; }

    // Conteos
    public int     DiasLaborales         { get; set; }   // días hábiles en el período
    public int     DiasTrabajados        { get; set; }   // días con entrada registrada
    public int     DiasAusente           { get; set; }
    public int     DiasTardanza          { get; set; }
    public int     DiasATiempo           { get; set; }

    // Minutos
    public int     TotalMinutosTardanza  { get; set; }
    public double  PromedioMinutosTardanza { get; set; }

    // Porcentajes
    public double  PorcentajePuntualidad { get; set; }   // DiasATiempo / DiasTrabajados * 100
    public double  PorcentajeAsistencia  { get; set; }   // DiasTrabajados / DiasLaborales * 100
}
