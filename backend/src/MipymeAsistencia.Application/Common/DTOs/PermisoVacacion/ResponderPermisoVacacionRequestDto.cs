namespace MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

public class ResponderPermisoVacacionRequestDto
{
    public int IdUsuarioAprobador { get; set; }
    public string EstadoSolicitud { get; set; } = "Aceptado";
}
