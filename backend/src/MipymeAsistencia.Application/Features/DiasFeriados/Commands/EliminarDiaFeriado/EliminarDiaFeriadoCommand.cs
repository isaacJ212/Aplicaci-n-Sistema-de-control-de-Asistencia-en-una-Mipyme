using MediatR;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Commands.EliminarDiaFeriado;

public class EliminarDiaFeriadoCommand : IRequest<bool>
{
    public int IdDiaFeriado { get; set; }
}
