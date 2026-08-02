using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.CreateEmpleado;

public class CreateEmpleadoCommandHandler : IRequestHandler<CreateEmpleadoCommand, EmpleadoResponseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEmpleadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoResponseDto> Handle(CreateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u => u.IdUsuario == request.IdUsuario, cancellationToken);

        if (!usuarioExiste)
            throw new KeyNotFoundException("El usuario asociado no existe.");

        var empleadoExiste = await _context.Empleados
            .AnyAsync(e => e.IdUsuario == request.IdUsuario || e.CedulaIdentificacion == request.CedulaIdentificacion, cancellationToken);

        if (empleadoExiste)
            throw new InvalidOperationException("Ya existe un empleado con ese usuario o cédula.");

        var empleado = new Domain.Entities.Empleado
        {
            IdUsuario = request.IdUsuario,
            CedulaIdentificacion = request.CedulaIdentificacion,
            FotoUrl = request.FotoUrl,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            CargoFuncion = request.CargoFuncion,
            Responsabilidades = request.Responsabilidades,
            FechaContratacion = request.FechaContratacion,
            SalarioBaseMensual = request.SalarioBaseMensual,
            DiasVacacionesAcumuladas = request.DiasVacacionesAcumuladas
        };

        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync(cancellationToken);

        return new EmpleadoResponseDto
        {
            IdEmpleado = empleado.IdEmpleado,
            IdUsuario = empleado.IdUsuario,
            CedulaIdentificacion = empleado.CedulaIdentificacion,
            FotoUrl = empleado.FotoUrl,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            CargoFuncion = empleado.CargoFuncion,
            Responsabilidades = empleado.Responsabilidades,
            FechaContratacion = empleado.FechaContratacion,
            SalarioBaseMensual = empleado.SalarioBaseMensual,
            DiasVacacionesAcumuladas = empleado.DiasVacacionesAcumuladas,
            Email = (await _context.Usuarios.FirstAsync(u => u.IdUsuario == empleado.IdUsuario, cancellationToken)).Email
        };
    }
}
