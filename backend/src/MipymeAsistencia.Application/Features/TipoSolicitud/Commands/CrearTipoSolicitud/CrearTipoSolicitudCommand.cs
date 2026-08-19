using MediatR;
using MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.TipoSolicitud.Commands.CrearTipoSolicitud;

public class CrearTipoSolicitudCommand : IRequest<TipoSolicitudPermisoDto>
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool DescuentaVacaciones { get; set; }
    public bool PermitePorHoras { get; set; }
    public int? MaximoDiasPorSolicitud { get; set; }
    public string? Icono { get; set; }
    public bool Activo { get; set; } = true;
}

public class CrearTipoSolicitudCommandHandler : IRequestHandler<CrearTipoSolicitudCommand, TipoSolicitudPermisoDto>
{
    private readonly IApplicationDbContext _context;

    public CrearTipoSolicitudCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<TipoSolicitudPermisoDto> Handle(CrearTipoSolicitudCommand request, CancellationToken cancellationToken)
    {
        var nombreNorm = request.Nombre.Trim();

        var existe = await _context.TiposSolicitudPermiso
            .AnyAsync(t => t.Nombre.ToLower() == nombreNorm.ToLower(), cancellationToken);

        if (existe)
            throw new InvalidOperationException($"Ya existe un tipo de solicitud con el nombre '{nombreNorm}'.");

        var tipo = new TipoSolicitudPermiso
        {
            Nombre                 = nombreNorm,
            Descripcion            = request.Descripcion?.Trim(),
            RequiereComprobante    = request.RequiereComprobante,
            DescuentaVacaciones    = request.DescuentaVacaciones,
            PermitePorHoras        = request.PermitePorHoras,
            MaximoDiasPorSolicitud = request.MaximoDiasPorSolicitud,
            Icono                  = string.IsNullOrWhiteSpace(request.Icono) ? "calendar" : request.Icono.Trim(),
            Activo                 = request.Activo
        };

        _context.TiposSolicitudPermiso.Add(tipo);
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
