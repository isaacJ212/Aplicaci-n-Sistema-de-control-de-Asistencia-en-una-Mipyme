using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Queries.GetSolicitudesPermisoVacacion;

public class GetSolicitudesPermisoVacacionQueryHandler : IRequestHandler<GetSolicitudesPermisoVacacionQuery, List<PermisoVacacionResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSolicitudesPermisoVacacionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermisoVacacionResponseDto>> Handle(GetSolicitudesPermisoVacacionQuery request, CancellationToken cancellationToken)
    {
        var query = _context.HistorialPermisosVacaciones
            .Include(x => x.Empleado)
            .AsQueryable();

        if (request.IdEmpleado.HasValue)
            query = query.Where(x => x.IdEmpleado == request.IdEmpleado.Value);

        if (!string.IsNullOrWhiteSpace(request.EstadoSolicitud))
            query = query.Where(x => x.EstadoSolicitud == request.EstadoSolicitud);

        if (!string.IsNullOrWhiteSpace(request.TipoSolicitud))
            query = query.Where(x => x.TipoSolicitud == request.TipoSolicitud);

        var solicitudes = await query
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync(cancellationToken);

        return solicitudes.Select(x => new PermisoVacacionResponseDto
        {
            IdSolicitud = x.IdSolicitud,
            IdEmpleado = x.IdEmpleado,
            NombreEmpleado = x.Empleado is null ? string.Empty : $"{x.Empleado.Nombres} {x.Empleado.Apellidos}".Trim(),
            TipoSolicitud = x.TipoSolicitud,
            FechaInicio = x.FechaInicio,
            FechaFin = x.FechaFin,
            DiasSolicitados = x.DiasSolicitados,
            Motivo = x.Motivo,
            EstadoSolicitud = x.EstadoSolicitud,
            FechaRespuesta = x.FechaRespuesta,
            IdUsuarioAprobador = x.IdUsuarioAprobador
        }).ToList();
    }
}
