using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CerrarPeriodo;

public class CerrarPeriodoCommand : IRequest<PeriodoCierreDto>
{
    public string Periodo { get; set; } = string.Empty;
    public int? IdUsuarioCierre { get; set; }
    public string? Observaciones { get; set; }
}
