using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;

namespace MipymeAsistencia.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<LoginResponseDto>
{
    public string RefreshToken { get; set; } = string.Empty;
}
