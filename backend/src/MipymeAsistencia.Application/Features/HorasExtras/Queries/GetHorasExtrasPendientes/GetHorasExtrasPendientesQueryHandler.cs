using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasPendientes;

public class GetHorasExtrasPendientesQueryHandler
    : IRequestHandler<GetHorasExtrasPendientesQuery, List<HoraExtraResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHorasExtrasPendientesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<HoraExtraResponseDto>> Handle(
        GetHorasExtrasPendientesQuery request, CancellationToken cancellationToken)
    {
        return await _context.HorasExtras
            .Include(h => h.Empleado)
            .Where(h => h.Estado == "Pendiente")
            .OrderByDescending(h => h.Fecha)
            .Select(h => new HoraExtraResponseDto
            {
                IdHoraExtra        = h.IdHoraExtra,
                IdEmpleado         = h.IdEmpleado,
                NombreEmpleado     = h.Empleado!.Nombres + " " + h.Empleado.Apellidos,
                IdUsuarioAprobador = h.IdUsuarioAprobador,
                NombreAprobador    = null,
                Fecha              = h.Fecha,
                CantidadHoras      = h.CantidadHoras,
                Motivo             = h.Motivo,
                MontoPagar         = h.MontoPagar,
                Estado             = h.Estado
            })
            .ToListAsync(cancellationToken);
    }
}
