using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;

namespace MipymeAsistencia.Application.Features.Planilla.Queries.GetPlanillasByEmpleado;

/// <summary>
/// Retorna el historial de planillas de un empleado, ordenado del más reciente al más antiguo.
/// Opcionalmente se puede filtrar por periodo (YYYY-MM).
/// </summary>
public class GetPlanillasByEmpleadoQuery : IRequest<List<PlanillaResponseDto>>
{
    public int     IdEmpleado    { get; set; }

    /// <summary>Filtro opcional por periodo YYYY-MM.</summary>
    public string? PeriodoMesAnio { get; set; }
}
