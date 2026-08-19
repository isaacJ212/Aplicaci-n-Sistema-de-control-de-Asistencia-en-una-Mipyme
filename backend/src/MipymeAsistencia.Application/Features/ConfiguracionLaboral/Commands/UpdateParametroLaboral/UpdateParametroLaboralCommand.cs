using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.UpdateParametroLaboral;

public class UpdateParametroLaboralCommand : IRequest<ParametroLaboralDto>
{
    public string Clave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? Descripcion { get; set; }
}
