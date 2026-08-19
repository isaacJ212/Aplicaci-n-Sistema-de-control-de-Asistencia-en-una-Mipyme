using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.ReabrirPeriodo;

public class ReabrirPeriodoCommand : IRequest<PeriodoCierreDto>
{
    public string Periodo { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}
