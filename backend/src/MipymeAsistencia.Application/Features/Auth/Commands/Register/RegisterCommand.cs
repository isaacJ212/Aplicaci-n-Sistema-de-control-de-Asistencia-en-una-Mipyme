using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<RegisterResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Empleado";
}
