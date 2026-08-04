namespace MipymeAsistencia.Application.Common.DTOs.Asistencia;

/// <summary>
/// Empleado reincidente en tardanzas: más de N llegadas tardías en el mes.
/// Usado por el dashboard del Admin para generar alertas de puntualidad.
/// </summary>
public class AlertaTardanzaDto
{
    public int    IdEmpleado       { get; set; }
    public string NombreEmpleado   { get; set; } = string.Empty;
    public string CargoFuncion     { get; set; } = string.Empty;
    public string PeriodoMesAnio   { get; set; } = string.Empty;
    public int    TotalTardanzas   { get; set; }
    public int    TotalMinutos     { get; set; }

    /// <summary>Monto descontado de la planilla por tardanzas (C$).</summary>
    public decimal DeduccionTardanza { get; set; }

    /// <summary>true si supera el umbral de reincidencia (por defecto 3).</summary>
    public bool EsReincidente { get; set; }
}
