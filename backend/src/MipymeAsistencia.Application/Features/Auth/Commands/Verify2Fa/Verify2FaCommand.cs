using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;

namespace MipymeAsistencia.Application.Features.Auth.Commands.Verify2Fa;

public class Verify2FaCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IpOrigen { get; set; }
    public string? MacAddress { get; set; }
}
