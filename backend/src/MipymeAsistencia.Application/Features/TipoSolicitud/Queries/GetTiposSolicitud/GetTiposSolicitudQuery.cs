using MediatR;
using MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.TipoSolicitud.Queries.GetTiposSolicitud;

public class GetTiposSolicitudQuery : IRequest<List<TipoSolicitudPermisoDto>>
{
    public bool? SoloActivos { get; set; } = true;
}

public class GetTiposSolicitudQueryHandler : IRequestHandler<GetTiposSolicitudQuery, List<TipoSolicitudPermisoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTiposSolicitudQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<TipoSolicitudPermisoDto>> Handle(GetTiposSolicitudQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TiposSolicitudPermiso.AsNoTracking();

        if (request.SoloActivos.HasValue && request.SoloActivos.Value)
        {
            query = query.Where(t => t.Activo);
        }

        var list = await query
            .OrderBy(t => t.IdTipoSolicitud)
            .ToListAsync(cancellationToken);

        return list.Select(t => new TipoSolicitudPermisoDto
        {
            IdTipoSolicitud        = t.IdTipoSolicitud,
            Nombre                 = t.Nombre,
            Descripcion            = t.Descripcion,
            RequiereComprobante    = t.RequiereComprobante,
            DescuentaVacaciones    = t.DescuentaVacaciones,
            PermitePorHoras        = t.PermitePorHoras,
            MaximoDiasPorSolicitud = t.MaximoDiasPorSolicitud,
            Icono                  = t.Icono,
            Activo                 = t.Activo
        }).ToList();
    }
}
