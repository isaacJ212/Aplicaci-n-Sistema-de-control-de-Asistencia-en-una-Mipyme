using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.UpdateEmpleado;

public class UpdateEmpleadoCommandHandler : IRequestHandler<UpdateEmpleadoCommand, EmpleadoResponseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmpleadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoResponseDto> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("No existe el empleado solicitado.");

        var existeCedula = await _context.Empleados
            .AnyAsync(e => e.CedulaIdentificacion == request.CedulaIdentificacion && e.IdEmpleado != request.IdEmpleado, cancellationToken);

        if (existeCedula)
            throw new InvalidOperationException("La cédula ya está registrada para otro empleado.");

        var fechaContratacionUtc = NormalizeToUtc(request.FechaContratacion);

        empleado.CedulaIdentificacion = request.CedulaIdentificacion;
        empleado.NumeroInss = request.NumeroInss;
        empleado.EstadoCivil = request.EstadoCivil;
        empleado.EstadoEmpleado = request.EstadoEmpleado;
        empleado.FotoUrl = request.FotoUrl;
        empleado.Nombres = request.Nombres;
        empleado.Apellidos = request.Apellidos;
        empleado.CargoFuncion = request.CargoFuncion;
        empleado.Departamento = string.IsNullOrWhiteSpace(request.Departamento) ? "General" : request.Departamento.Trim();
        empleado.Responsabilidades = request.Responsabilidades;
        empleado.FechaContratacion = fechaContratacionUtc;
        empleado.SalarioBaseMensual = request.SalarioBaseMensual;
        empleado.DiasVacacionesAcumuladas = request.DiasVacacionesAcumuladas;

        await _context.SaveChangesAsync(cancellationToken);

        var usuario = await _context.Usuarios.FirstAsync(u => u.IdUsuario == empleado.IdUsuario, cancellationToken);

        return new EmpleadoResponseDto
        {
            IdEmpleado = empleado.IdEmpleado,
            IdUsuario = empleado.IdUsuario,
            Email = usuario.Email,
            CedulaIdentificacion = empleado.CedulaIdentificacion,
            NumeroInss = empleado.NumeroInss,
            EstadoCivil = empleado.EstadoCivil,
            EstadoEmpleado = empleado.EstadoEmpleado,
            FotoUrl = empleado.FotoUrl,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            CargoFuncion = empleado.CargoFuncion,
            Departamento = empleado.Departamento,
            Responsabilidades = empleado.Responsabilidades,
            FechaContratacion = empleado.FechaContratacion,
            SalarioBaseMensual = empleado.SalarioBaseMensual,
            DiasVacacionesAcumuladas = empleado.DiasVacacionesAcumuladas
        };
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
