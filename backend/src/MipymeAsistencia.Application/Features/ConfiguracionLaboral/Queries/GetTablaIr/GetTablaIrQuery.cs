using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetTablaIr;

public class GetTablaIrQuery : IRequest<List<TablaIrDto>>
{
    public int? Anio { get; set; }
    public bool SoloActivos { get; set; } = true;
}
