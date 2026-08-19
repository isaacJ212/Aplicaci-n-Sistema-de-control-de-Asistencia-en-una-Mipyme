using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluacionById;

public class GetEvaluacionByIdQuery : IRequest<EvaluacionResponseDto>
{
    public int IdEvaluacion { get; set; }
}
