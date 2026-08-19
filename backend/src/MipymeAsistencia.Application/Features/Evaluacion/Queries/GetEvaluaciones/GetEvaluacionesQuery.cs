using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluaciones;

public class GetEvaluacionesQuery : IRequest<List<EvaluacionResponseDto>>
{
    public int?    IdEmpleado  { get; set; }
    public int?    IdEvaluador { get; set; }
    public string? Periodo     { get; set; }
    public string? Estado      { get; set; }
}
