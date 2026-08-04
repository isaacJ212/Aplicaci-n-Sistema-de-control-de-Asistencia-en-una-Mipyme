using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetAllAsistencias;

/// <summary>
/// Query para obtener todos los registros de asistencia del sistema con filtros opcionales.
/// Solo accesible por Admin.
/// </summary>
public class GetAllAsistenciasQuery : IRequest<List<AsistenciaResponseDto>>
{
    /// <summary>Filtrar por empleado (opcional).</summary>
    public int? IdEmpleado { get; set; }

    /// <summary>Filtrar desde esta fecha (opcional, formato UTC).</summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>Filtrar hasta esta fecha (opcional, formato UTC).</summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>Filtrar por estado: "A Tiempo", "Tardanza", "Ausente" (opcional).</summary>
    public string? EstadoAsistencia { get; set; }
}
