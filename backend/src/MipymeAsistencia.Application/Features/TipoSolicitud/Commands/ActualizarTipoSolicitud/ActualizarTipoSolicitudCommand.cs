using MediatR;
using MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.TipoSolicitud.Commands.ActualizarTipoSolicitud;

public class ActualizarTipoSolicitudCommand : IRequest<TipoSolicitudPermisoDto>
{
    public int IdTipoSolicitud { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool DescuentaVacaciones { get; set; }
    public bool PermitePorHoras { get; set; }
    public int? MaximoDiasPorSolicitud { get; set; }
    public string? Icono { get; set; }
    public bool Activo { get; set; }
}

public class ActualizarTipoSolicitudCommandHandler : IRequestHandler<ActualizarTipoSolicitudCommand, TipoSolicitudPermisoDto>
{
    private readonly IApplicationDbContext _context;

    public ActualizarTipoSolicitudCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<TipoSolicitudPermisoDto> Handle(ActualizarTipoSolicitudCommand request, CancellationToken cancellationToken)
    {
        var tipo = await _context.TiposSolicitudPermiso
            .FirstOrDefaultAsync(t => t.IdTipoSolicitud == request.IdTipoSolicitud, cancellationToken);

        if (tipo is null)
            throw new KeyNotFoundException($"Tipo de solicitud #{request.IdTipoSolicitud} no encontrado.");

        var nombreNorm = request.Nombre.Trim();

        var nombreDuplicado = await _context.TiposSolicitudPermiso
            .AnyAsync(t => t.IdTipoSolicitud != request.IdTipoSolicitud && t.Nombre.ToLower() == nombreNorm.ToLower(), cancellationToken);

        if (nombreDuplicado)
            throw new InvalidOperationException($"Ya existe otro tipo de solicitud con el nombre '{nombreNorm}'.");

        tipo.Nombre                 = nombreNorm;
        tipo.Descripcion            = request.Descripcion?.Trim();
        tipo.RequiereComprobante    = request.RequiereComprobante;
        tipo.DescuentaVacaciones    = request.DescuentaVacaciones;
        tipo.PermitePorHoras        = request.PermitePorHoras;
        tipo.MaximoDiasPorSolicitud = request.MaximoDiasPorSolicitud;
        tipo.Icono                  = string.IsNullOrWhiteSpace(request.Icono) ? "calendar" : request.Icono.Trim();
        tipo.Activo                 = request.Activo;

        await _context.SaveChangesAsync(cancellationToken);

        return new TipoSolicitudPermisoDto
        {
            IdTipoSolicitud        = tipo.IdTipoSolicitud,
            Nombre                 = tipo.Nombre,
            Descripcion            = tipo.Descripcion,
            RequiereComprobante    = tipo.RequiereComprobante,
            DescuentaVacaciones    = tipo.DescuentaVacaciones,
            PermitePorHoras        = tipo.PermitePorHoras,
            MaximoDiasPorSolicitud = tipo.MaximoDiasPorSolicitud,
            Icono                  = tipo.Icono,
            Activo                 = tipo.Activo
        };
    }
}
