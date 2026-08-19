using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CrearPeriodoCierre;

public class CrearPeriodoCierreCommand : IRequest<PeriodoCierreDto>
{
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaCorteHorasExtras { get; set; }
    public DateTime FechaEmisionPlanilla { get; set; }
    public string? Observaciones { get; set; }
}
