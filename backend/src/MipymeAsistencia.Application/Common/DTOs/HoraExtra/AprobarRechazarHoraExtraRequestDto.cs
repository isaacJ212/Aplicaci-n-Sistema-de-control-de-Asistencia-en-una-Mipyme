namespace MipymeAsistencia.Application.Common.DTOs.HoraExtra;

/// <summary>
/// Payload para que un Admin apruebe o rechace una hora extra pendiente.
/// </summary>
public class AprobarRechazarHoraExtraRequestDto
{
    /// <summary>Aprobado | Rechazado</summary>
    public string Estado { get; set; } = string.Empty;
}
