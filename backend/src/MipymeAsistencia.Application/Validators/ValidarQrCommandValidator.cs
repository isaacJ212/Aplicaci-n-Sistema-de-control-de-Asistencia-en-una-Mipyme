using FluentValidation;
using MipymeAsistencia.Application.Features.Asistencia.Commands.ValidarQr;

namespace MipymeAsistencia.Application.Validators;

public class ValidarQrCommandValidator : AbstractValidator<ValidarQrCommand>
{
    public ValidarQrCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El id del empleado es obligatorio.");

        RuleFor(x => x.TokenQrEscaneado)
            .NotEmpty().WithMessage("El token QR es obligatorio.");
    }
}
