using FluentValidation;
using MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;
using System.Text.RegularExpressions;

namespace MipymeAsistencia.Application.Validators;

public class GenerarPlanillaCommandValidator : AbstractValidator<GenerarPlanillaCommand>
{
    public GenerarPlanillaCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El id del empleado es obligatorio.");

        RuleFor(x => x.PeriodoMesAnio)
            .NotEmpty().WithMessage("El periodo es obligatorio.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
            .WithMessage("El periodo debe tener el formato YYYY-MM (ej. 2026-05).");

        RuleFor(x => x.Comisiones)
            .GreaterThanOrEqualTo(0).WithMessage("Las comisiones no pueden ser negativas.");

        RuleFor(x => x.Incentivos)
            .GreaterThanOrEqualTo(0).WithMessage("Los incentivos no pueden ser negativos.");

        RuleFor(x => x.Embargo)
            .GreaterThanOrEqualTo(0).WithMessage("El embargo no puede ser negativo.");

        RuleFor(x => x.Sindicato)
            .GreaterThanOrEqualTo(0).WithMessage("La cuota sindical no puede ser negativa.");

        RuleFor(x => x.OtrasDeducciones)
            .GreaterThanOrEqualTo(0).WithMessage("Las deducciones adicionales no pueden ser negativas.");
    }
}
