using FluentValidation;
using MipymeAsistencia.Application.Features.Auth.Commands.Logout;

namespace MipymeAsistencia.Application.Validators;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio para cerrar sesión.");
    }
}
