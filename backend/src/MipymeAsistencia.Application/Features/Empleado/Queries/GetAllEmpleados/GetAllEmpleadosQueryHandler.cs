using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Empleado.Queries.GetAllEmpleados;

public class GetAllEmpleadosQueryHandler : IRequestHandler<GetAllEmpleadosQuery, List<EmpleadoResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEmpleadosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpleadoResponseDto>> Handle(GetAllEmpleadosQuery request, CancellationToken cancellationToken)
    {
        var empleados = await _context.Empleados
            .Include(e => e.Usuario)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync(cancellationToken);

        return empleados.Select(e => new EmpleadoResponseDto
        {
            IdEmpleado = e.IdEmpleado,
            IdUsuario = e.IdUsuario,
            Email = e.Usuario?.Email ?? string.Empty,
            CedulaIdentificacion = e.CedulaIdentificacion,
            NumeroInss = e.NumeroInss,
            EstadoCivil = e.EstadoCivil,
            EstadoEmpleado = e.EstadoEmpleado,
            FotoUrl = e.FotoUrl,
            Nombres = e.Nombres,
            Apellidos = e.Apellidos,
            CargoFuncion = e.CargoFuncion,
            Departamento = e.Departamento,
            Responsabilidades = e.Responsabilidades,
            FechaContratacion = e.FechaContratacion,
            SalarioBaseMensual = e.SalarioBaseMensual,
            DiasVacacionesAcumuladas = e.DiasVacacionesAcumuladas
        }).ToList();
    }
}
