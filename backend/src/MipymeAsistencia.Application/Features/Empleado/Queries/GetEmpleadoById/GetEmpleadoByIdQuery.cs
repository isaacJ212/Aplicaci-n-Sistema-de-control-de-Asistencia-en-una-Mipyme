using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;

namespace MipymeAsistencia.Application.Features.Empleado.Queries.GetEmpleadoById;

public class GetEmpleadoByIdQuery : IRequest<EmpleadoResponseDto>
{
    public int IdEmpleado { get; set; }
}
