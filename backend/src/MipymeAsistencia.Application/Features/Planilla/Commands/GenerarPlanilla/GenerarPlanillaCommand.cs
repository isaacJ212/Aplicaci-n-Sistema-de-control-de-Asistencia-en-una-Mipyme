using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;

namespace MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;

public class GenerarPlanillaCommand : IRequest<PlanillaResponseDto>
{
    public int     IdEmpleado       { get; set; }
    public string  PeriodoMesAnio   { get; set; } = string.Empty;
    public decimal Comisiones       { get; set; } = 0m;
    public decimal Incentivos       { get; set; } = 0m;
    public decimal Embargo          { get; set; } = 0m;
    public decimal Sindicato        { get; set; } = 0m;
    public decimal OtrasDeducciones { get; set; } = 0m;
}
