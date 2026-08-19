using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiasFeriados;

public class GetDiasFeriadosQuery : IRequest<List<DiaFeriadoDto>>
{
    public int? Anio { get; set; }
}
