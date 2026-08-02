using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetHistorialAsistencia;

public class GetHistorialAsistenciaQuery : IRequest<List<AsistenciaResponseDto>>
{
    public int IdEmpleado { get; set; }
}
