namespace MipymeAsistencia.Application.Common.DTOs.Sede;

/// <summary>
/// Payload para actualizar la configuración de la sede.
/// Todos los campos son requeridos en la actualización (PUT completo).
/// </summary>
public class UpdateSedeRequestDto
{
    public string NombreSede { get; set; } = string.Empty;
    public decimal LatitudSede { get; set; }
    public decimal LongitudSede { get; set; }
    public int RadioToleranciaMetros { get; set; }

    /// <summary>Formato esperado: HH:mm (ej. "08:00").</summary>
    public string HoraEntradaOficial { get; set; } = string.Empty;

    /// <summary>Formato esperado: HH:mm (ej. "17:00").</summary>
    public string HoraSalidaOficial { get; set; } = string.Empty;

    public int DuracionAlmuerzoMinutos { get; set; }
    public int MinutosTolerancia { get; set; }
}
