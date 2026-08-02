using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.DeleteEmpleado;

public class DeleteEmpleadoCommandHandler : IRequestHandler<DeleteEmpleadoCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmpleadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
