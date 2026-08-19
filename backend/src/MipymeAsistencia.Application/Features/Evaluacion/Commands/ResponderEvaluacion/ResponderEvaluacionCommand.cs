using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;

namespace MipymeAsistencia.Application.Features.Evaluacion.Commands.ResponderEvaluacion;

public class ResponderEvaluacionCommand : IRequest<EvaluacionResponseDto>
{
    public int                IdEvaluacion  { get; set; }
    public List<RespuestaDto> Respuestas    { get; set; } = [];
    public string?            Observaciones { get; set; }
}
