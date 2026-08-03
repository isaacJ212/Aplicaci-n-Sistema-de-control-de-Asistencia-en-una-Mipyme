using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Commands.ResponderPermisoVacacion;

public class ResponderPermisoVacacionCommand : IRequest<PermisoVacacionResponseDto>
{
    public int IdSolicitud { get; set; }
    public int IdUsuarioAprobador { get; set; }
    public string EstadoSolicitud { get; set; } = "Aceptado";
}
