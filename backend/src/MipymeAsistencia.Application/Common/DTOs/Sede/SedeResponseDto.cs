namespace MipymeAsistencia.Application.Common.DTOs.Sede;

/// <summary>
/// Datos de la configuración de sede devueltos al cliente.
/// Incluye coordenadas GPS, horarios oficiales y estado del QR dinámico.
/// </summary>
public class SedeResponseDto
{
    public int IdSede { get; set; }
    public string NombreSede { get; set; } = string.Empty;
    public decimal LatitudSede { get; set; }
    public decimal LongitudSede { get; set; }
    public int RadioToleranciaMetros { get; set; }

    /// <summary>Formato HH:mm — hora de entrada oficial.</summary>
    public string HoraEntradaOficial { get; set; } = string.Empty;

    /// <summary>Formato HH:mm — hora de salida oficial.</summary>
    public string HoraSalidaOficial { get; set; } = string.Empty;

    public int DuracionAlmuerzoMinutos { get; set; }
    public int MinutosTolerancia { get; set; }

    /// <summary>Token QR activo en la pantalla de marcaje. Null si aún no se ha generado.</summary>
    public string? TokenQrActual { get; set; }

    public DateTime? QrUltimaActualizacion { get; set; }
}
