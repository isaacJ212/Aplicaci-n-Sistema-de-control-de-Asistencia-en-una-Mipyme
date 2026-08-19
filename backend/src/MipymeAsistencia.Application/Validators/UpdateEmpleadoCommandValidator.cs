using FluentValidation;
using MipymeAsistencia.Application.Features.Empleado.Commands.UpdateEmpleado;

namespace MipymeAsistencia.Application.Validators;

public class UpdateEmpleadoCommandValidator : AbstractValidator<UpdateEmpleadoCommand>
{
    public UpdateEmpleadoCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El identificador del empleado es obligatorio.");

        RuleFor(x => x.CedulaIdentificacion)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .MaximumLength(20).WithMessage("La cédula no puede exceder 20 caracteres.");

        RuleFor(x => x.NumeroInss)
            .NotEmpty().WithMessage("El número de INSS es obligatorio.")
            .Matches("^\\d{9}$").WithMessage("El número de INSS debe tener exactamente 9 dígitos.");

        RuleFor(x => x.EstadoCivil)
            .NotEmpty().WithMessage("El estado civil es obligatorio.")
            .Must(v => new[] { "Soltero", "Casado", "Divorciado", "Viudo", "Unión libre" }.Contains(v))
            .WithMessage("El estado civil no es válido.");

        RuleFor(x => x.EstadoEmpleado)
            .NotEmpty().WithMessage("El estado del empleado es obligatorio.")
            .Must(v => new[] { "Activo", "Inactivo", "Suspendido", "Embargado" }.Contains(v))
            .WithMessage("El estado del empleado no es válido.");

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
