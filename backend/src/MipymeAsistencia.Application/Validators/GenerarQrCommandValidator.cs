using FluentValidation;
using MipymeAsistencia.Application.Features.Asistencia.Commands.GenerarQr;

namespace MipymeAsistencia.Application.Validators;

public class GenerarQrCommandValidator : AbstractValidator<GenerarQrCommand>
{
    public GenerarQrCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El id del empleado es obligatorio.");
    }
}
