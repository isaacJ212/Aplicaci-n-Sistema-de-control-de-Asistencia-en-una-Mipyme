using FluentValidation;
using MipymeAsistencia.Application.Features.Auth.Commands.RefreshToken;

namespace MipymeAsistencia.Application.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio.");
    }
}
