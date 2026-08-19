using MediatR;
using MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.TipoSolicitud.Queries.GetTipoSolicitudById;

public class GetTipoSolicitudByIdQuery : IRequest<TipoSolicitudPermisoDto>
{
    public int IdTipoSolicitud { get; set; }
}

public class GetTipoSolicitudByIdQueryHandler : IRequestHandler<GetTipoSolicitudByIdQuery, TipoSolicitudPermisoDto>
{
    private readonly IApplicationDbContext _context;

    public GetTipoSolicitudByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<TipoSolicitudPermisoDto> Handle(GetTipoSolicitudByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.TiposSolicitudPermiso
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdTipoSolicitud == request.IdTipoSolicitud, cancellationToken);

        if (t is null)
            throw new KeyNotFoundException($"Tipo de solicitud #{request.IdTipoSolicitud} no encontrado.");

        return new TipoSolicitudPermisoDto
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
        };
    }
}
