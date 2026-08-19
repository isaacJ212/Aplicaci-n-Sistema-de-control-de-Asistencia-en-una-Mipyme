using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.EsDiaFeriado;

public class EsDiaFeriadoQuery : IRequest<DiaFeriadoDto?>
{
    public DateTime Fecha { get; set; }
}
