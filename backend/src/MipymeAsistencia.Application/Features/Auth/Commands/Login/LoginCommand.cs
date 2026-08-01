using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
