using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Queries.GetEmpleadoById;

public class GetEmpleadoByIdQueryHandler : IRequestHandler<GetEmpleadoByIdQuery, EmpleadoResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmpleadoByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoResponseDto> Handle(GetEmpleadoByIdQuery request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        return new EmpleadoResponseDto
        {
            IdEmpleado = empleado.IdEmpleado,
            IdUsuario = empleado.IdUsuario,
            Email = empleado.Usuario?.Email ?? string.Empty,
            CedulaIdentificacion = empleado.CedulaIdentificacion,
            FotoUrl = empleado.FotoUrl,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            CargoFuncion = empleado.CargoFuncion,
            Responsabilidades = empleado.Responsabilidades,
            FechaContratacion = empleado.FechaContratacion,
            SalarioBaseMensual = empleado.SalarioBaseMensual,
            DiasVacacionesAcumuladas = empleado.DiasVacacionesAcumuladas
        };
    }
}
