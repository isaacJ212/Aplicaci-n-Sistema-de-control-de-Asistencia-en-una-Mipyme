using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Commands.SolicitarPermisoVacacion;

public class SolicitarPermisoVacacionCommand : IRequest<PermisoVacacionResponseDto>
{
    public int IdEmpleado { get; set; }
    public string TipoSolicitud { get; set; } = "Permiso";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal? DiasSolicitados { get; set; }
}
