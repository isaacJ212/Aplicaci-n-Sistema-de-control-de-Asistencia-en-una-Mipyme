using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;

namespace MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanillaPorDepartamento;

public class GenerarPlanillaPorDepartamentoCommand : IRequest<GenerarPlanillaPorDepartamentoResponseDto>
{
    public string PeriodoMesAnio { get; set; } = string.Empty;
    public string? Departamento { get; set; } = "Todos";
    public decimal ComisionesGenerales { get; set; } = 0m;
    public decimal IncentivosGenerales { get; set; } = 0m;
    public decimal OtrasDeduccionesGenerales { get; set; } = 0m;
}
