using FluentValidation;
using MipymeAsistencia.Application.Features.Empleado.Commands.CreateEmpleado;

namespace MipymeAsistencia.Application.Validators;

public class CreateEmpleadoCommandValidator : AbstractValidator<CreateEmpleadoCommand>
{
    public CreateEmpleadoCommandValidator()
    {
        RuleFor(x => x.IdUsuario)
            .GreaterThan(0).WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.CedulaIdentificacion)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .MaximumLength(20).WithMessage("La cédula no puede exceder 20 caracteres.");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(100).WithMessage("Los nombres no pueden exceder 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder 100 caracteres.");

        RuleFor(x => x.CargoFuncion)
            .NotEmpty().WithMessage("El cargo o función es obligatorio.")
            .MaximumLength(100).WithMessage("El cargo no puede exceder 100 caracteres.");

        RuleFor(x => x.Responsabilidades)
            .NotEmpty().WithMessage("Las responsabilidades son requeridas.");

        RuleFor(x => x.FechaContratacion)
            .NotEmpty().WithMessage("La fecha de contratación es obligatoria.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha de contratación no puede estar en el futuro.");

        RuleFor(x => x.SalarioBaseMensual)
            .GreaterThan(0).WithMessage("El salario base debe ser mayor a 0.");

        RuleFor(x => x.DiasVacacionesAcumuladas)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de vacaciones no pueden ser negativos.");
    }
}
