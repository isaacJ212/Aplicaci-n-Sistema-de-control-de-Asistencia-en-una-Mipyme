using MediatR;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.GenerarQr;

public class GenerarQrCommand : IRequest<string>
{
    public int IdEmpleado { get; set; }
}
