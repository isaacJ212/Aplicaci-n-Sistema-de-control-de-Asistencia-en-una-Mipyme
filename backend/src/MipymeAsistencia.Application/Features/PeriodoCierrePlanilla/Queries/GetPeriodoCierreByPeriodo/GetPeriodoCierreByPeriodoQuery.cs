using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodoCierreByPeriodo;

public class GetPeriodoCierreByPeriodoQuery : IRequest<PeriodoCierreDto>
{
    public string Periodo { get; set; } = string.Empty;
}
