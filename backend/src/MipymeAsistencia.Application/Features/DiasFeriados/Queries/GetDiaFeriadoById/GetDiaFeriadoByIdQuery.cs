using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiaFeriadoById;

public class GetDiaFeriadoByIdQuery : IRequest<DiaFeriadoDto>
{
    public int IdDiaFeriado { get; set; }
}
