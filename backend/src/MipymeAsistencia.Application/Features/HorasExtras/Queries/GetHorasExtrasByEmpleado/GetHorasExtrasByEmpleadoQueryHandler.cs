using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasByEmpleado;

public class GetHorasExtrasByEmpleadoQueryHandler
    : IRequestHandler<GetHorasExtrasByEmpleadoQuery, List<HoraExtraResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHorasExtrasByEmpleadoQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<HoraExtraResponseDto>> Handle(
        GetHorasExtrasByEmpleadoQuery request, CancellationToken cancellationToken)
    {
        var empleadoExiste = await _context.Empleados
            .AnyAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (!empleadoExiste)
            throw new KeyNotFoundException($"Empleado con id {request.IdEmpleado} no encontrado.");

        return await _context.HorasExtras
            .Include(h => h.Empleado)
            .Include(h => h.UsuarioAprobador)
            .Where(h => h.IdEmpleado == request.IdEmpleado)
            .OrderByDescending(h => h.Fecha)
            .Select(h => new HoraExtraResponseDto
            {
                IdHoraExtra        = h.IdHoraExtra,
                IdEmpleado         = h.IdEmpleado,
                NombreEmpleado     = h.Empleado!.Nombres + " " + h.Empleado.Apellidos,
                IdUsuarioAprobador = h.IdUsuarioAprobador,
                NombreAprobador    = h.UsuarioAprobador != null ? h.UsuarioAprobador.Email : null,
                Fecha              = h.Fecha,
                CantidadHoras      = h.CantidadHoras,
                Motivo             = h.Motivo,
                MontoPagar         = h.MontoPagar,
                Estado             = h.Estado
            })
            .ToListAsync(cancellationToken);
    }
}
