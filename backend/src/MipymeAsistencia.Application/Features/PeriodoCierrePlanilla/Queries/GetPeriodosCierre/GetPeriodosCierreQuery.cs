using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodosCierre;

public class GetPeriodosCierreQuery : IRequest<List<PeriodoCierreDto>>
{
    public bool? SoloAbiertos { get; set; }
}
