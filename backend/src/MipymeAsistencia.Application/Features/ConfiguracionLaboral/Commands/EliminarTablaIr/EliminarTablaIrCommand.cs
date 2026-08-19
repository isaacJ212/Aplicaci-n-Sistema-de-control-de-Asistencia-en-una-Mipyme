using MediatR;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.EliminarTablaIr;

public class EliminarTablaIrCommand : IRequest<bool>
{
    public int IdTablaIr { get; set; }
}
