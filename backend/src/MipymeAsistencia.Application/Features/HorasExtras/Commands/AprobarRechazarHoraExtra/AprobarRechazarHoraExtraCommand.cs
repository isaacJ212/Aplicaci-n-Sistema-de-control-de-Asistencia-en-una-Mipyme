using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;

namespace MipymeAsistencia.Application.Features.HorasExtras.Commands.AprobarRechazarHoraExtra;

/// <summary>
/// Command para que un Admin apruebe o rechace una hora extra pendiente.
/// El IdUsuarioAprobador se extrae del JWT en el controlador.
/// </summary>
public class AprobarRechazarHoraExtraCommand : IRequest<HoraExtraResponseDto>
{
    public int    IdHoraExtra         { get; set; }
    public int    IdUsuarioAprobador  { get; set; }

    /// <summary>Aprobado | Rechazado</summary>
    public string Estado              { get; set; } = string.Empty;
}
