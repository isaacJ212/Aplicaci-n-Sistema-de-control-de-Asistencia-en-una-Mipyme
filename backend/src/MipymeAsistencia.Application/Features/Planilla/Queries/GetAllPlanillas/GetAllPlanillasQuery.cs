using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;

namespace MipymeAsistencia.Application.Features.Planilla.Queries.GetAllPlanillas;

/// <summary>
/// Query para obtener todas las planillas del sistema con filtros opcionales.
/// Solo accesible por Admin.
/// </summary>
public class GetAllPlanillasQuery : IRequest<List<PlanillaResponseDto>>
{
    /// <summary>Filtrar por periodo YYYY-MM (opcional).</summary>
    public string? PeriodoMesAnio { get; set; }

    /// <summary>Filtrar por departamento (opcional).</summary>
    public string? Departamento { get; set; }

    /// <summary>Filtrar por empleado específico (opcional).</summary>
    public int? IdEmpleado { get; set; }
}
