namespace MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

/// <summary>
/// Body para responder una solicitud de permiso/vacación.
/// El IdUsuarioAprobador se resuelve automáticamente desde el JWT en el controller.
/// </summary>
public class ResponderPermisoVacacionRequestDto
{
    /// <summary>'Aprobado' o 'Rechazado'</summary>
    public string EstadoSolicitud { get; set; } = "Aprobado";
}
