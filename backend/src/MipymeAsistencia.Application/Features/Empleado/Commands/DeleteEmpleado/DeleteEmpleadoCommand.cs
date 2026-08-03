using MediatR;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.DeleteEmpleado;

public class DeleteEmpleadoCommand : IRequest<bool>
{
    public int IdEmpleado { get; set; }
}
