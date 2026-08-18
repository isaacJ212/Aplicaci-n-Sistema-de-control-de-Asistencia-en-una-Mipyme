using MediatR;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Enable2Fa;

public class Enable2FaCommand : IRequest<object>
{
    public string Email { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
