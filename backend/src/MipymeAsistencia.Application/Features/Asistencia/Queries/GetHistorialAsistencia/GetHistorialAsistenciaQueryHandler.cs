using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetHistorialAsistencia;

public class GetHistorialAsistenciaQueryHandler : IRequestHandler<GetHistorialAsistenciaQuery, List<AsistenciaResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHistorialAsistenciaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AsistenciaResponseDto>> Handle(GetHistorialAsistenciaQuery request, CancellationToken cancellationToken)
    {
        var historial = await _context.HistorialAsistencias
            .Where(h => h.IdEmpleado == request.IdEmpleado)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync(cancellationToken);

        return historial.Select(h => new AsistenciaResponseDto
        {
            IdAsistencia = h.IdAsistencia,
            IdEmpleado = h.IdEmpleado,
            Fecha = h.Fecha,
            HoraEntrada = h.HoraEntrada.ToString(@"hh\:mm"),
            InicioAlmuerzo = h.InicioAlmuerzo?.ToString(@"hh\:mm"),
            FinAlmuerzo = h.FinAlmuerzo?.ToString(@"hh\:mm"),
            HoraSalida = h.HoraSalida?.ToString(@"hh\:mm"),
            LatitudMarcaje = h.LatitudMarcaje,
            LongitudMarcaje = h.LongitudMarcaje,
            DistanciaCalculadaMetros = h.DistanciaCalculadaMetros,
            EstadoAsistencia = h.EstadoAsistencia,
            MinutosTardanza = h.MinutosTardanza,
            EstaDentroDelRangoGps = h.EstaDentroDelRangoGps
        }).ToList();
    }
}
