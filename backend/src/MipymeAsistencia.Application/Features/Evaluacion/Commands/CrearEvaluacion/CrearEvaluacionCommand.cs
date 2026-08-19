using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;

namespace MipymeAsistencia.Application.Features.Evaluacion.Commands.CrearEvaluacion;

public class CrearEvaluacionCommand : IRequest<EvaluacionResponseDto>
{
    public int    IdEmpleado  { get; set; }
    public int    IdEvaluador { get; set; }
    public string Perspectiva { get; set; } = "Jefe";
    public string Periodo     { get; set; } = string.Empty;
}
