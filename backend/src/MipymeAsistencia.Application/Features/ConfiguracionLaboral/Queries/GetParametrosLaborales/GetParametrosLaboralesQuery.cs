using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetParametrosLaborales;

public class GetParametrosLaboralesQuery : IRequest<List<ParametroLaboralDto>>
{
}
