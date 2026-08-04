using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Queries.GetSolicitudesPermisoVacacion;

public class GetSolicitudesPermisoVacacionQuery : IRequest<List<PermisoVacacionResponseDto>>
{
    public int? IdEmpleado { get; set; }
    public string? EstadoSolicitud { get; set; }
    public string? TipoSolicitud { get; set; }
}
