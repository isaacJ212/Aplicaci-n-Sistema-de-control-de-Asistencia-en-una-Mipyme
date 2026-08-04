using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.PermisoVacacion.Commands.ResponderPermisoVacacion;

public class ResponderPermisoVacacionCommandHandler : IRequestHandler<ResponderPermisoVacacionCommand, PermisoVacacionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public ResponderPermisoVacacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermisoVacacionResponseDto> Handle(ResponderPermisoVacacionCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _context.HistorialPermisosVacaciones
            .Include(x => x.Empleado)
            .FirstOrDefaultAsync(x => x.IdSolicitud == request.IdSolicitud, cancellationToken);

        if (solicitud is null)
            throw new KeyNotFoundException("La solicitud no existe.");

        var aprobador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuarioAprobador, cancellationToken);

        if (aprobador is null)
            throw new KeyNotFoundException("El usuario aprobador no existe.");

        if (solicitud.EstadoSolicitud != "Pendiente")
            throw new InvalidOperationException("La solicitud ya fue respondida anteriormente.");

        var estado = request.EstadoSolicitud.Trim();
        if (estado != "Aceptado" && estado != "Denegado")
            throw new InvalidOperationException("El estado de la solicitud debe ser 'Aceptado' o 'Denegado'.");

        solicitud.EstadoSolicitud = estado;
        solicitud.IdUsuarioAprobador = request.IdUsuarioAprobador;
        solicitud.FechaRespuesta = DateTime.UtcNow;

        if (estado == "Aceptado" && solicitud.TipoSolicitud == "Vacacion")
        {
            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado, cancellationToken);

            if (empleado is null)
                throw new KeyNotFoundException("No se encontró el empleado asociado a la solicitud.");

            empleado.DiasVacacionesAcumuladas = Math.Max(0, empleado.DiasVacacionesAcumuladas - solicitud.DiasSolicitados);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PermisoVacacionResponseDto
        {
            IdSolicitud = solicitud.IdSolicitud,
            IdEmpleado = solicitud.IdEmpleado,
            NombreEmpleado = solicitud.Empleado is null ? string.Empty : $"{solicitud.Empleado.Nombres} {solicitud.Empleado.Apellidos}".Trim(),
            TipoSolicitud = solicitud.TipoSolicitud,
            FechaInicio = solicitud.FechaInicio,
            FechaFin = solicitud.FechaFin,
            DiasSolicitados = solicitud.DiasSolicitados,
            Motivo = solicitud.Motivo,
            EstadoSolicitud = solicitud.EstadoSolicitud,
            FechaRespuesta = solicitud.FechaRespuesta,
            IdUsuarioAprobador = solicitud.IdUsuarioAprobador
        };
    }
}
