using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetAllAsistencias;

public class GetAllAsistenciasQueryHandler
    : IRequestHandler<GetAllAsistenciasQuery, List<AsistenciaResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAsistenciasQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<AsistenciaResponseDto>> Handle(
        GetAllAsistenciasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.HistorialAsistencias
            .Include(a => a.Empleado)
            .AsQueryable();

        if (request.IdEmpleado.HasValue)
            query = query.Where(a => a.IdEmpleado == request.IdEmpleado.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= request.FechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(request.EstadoAsistencia))
            query = query.Where(a => a.EstadoAsistencia == request.EstadoAsistencia);

        return await query
            .OrderByDescending(a => a.Fecha)
            .Select(a => new AsistenciaResponseDto
            {
                IdAsistencia              = a.IdAsistencia,
                IdEmpleado                = a.IdEmpleado,
                Fecha                     = a.Fecha,
                HoraEntrada               = a.HoraEntrada.ToString(),
                InicioAlmuerzo            = a.InicioAlmuerzo.HasValue ? a.InicioAlmuerzo.Value.ToString() : null,
                FinAlmuerzo               = a.FinAlmuerzo.HasValue    ? a.FinAlmuerzo.Value.ToString()    : null,
                HoraSalida                = a.HoraSalida.HasValue     ? a.HoraSalida.Value.ToString()     : null,
                LatitudMarcaje            = a.LatitudMarcaje,
                LongitudMarcaje           = a.LongitudMarcaje,
                DistanciaCalculadaMetros  = a.DistanciaCalculadaMetros,
                EstadoAsistencia          = a.EstadoAsistencia,
                MinutosTardanza           = a.MinutosTardanza,
                EstaDentroDelRangoGps     = a.EstaDentroDelRangoGps,
                Mensaje                   = null
            })
            .ToListAsync(cancellationToken);
    }
}
