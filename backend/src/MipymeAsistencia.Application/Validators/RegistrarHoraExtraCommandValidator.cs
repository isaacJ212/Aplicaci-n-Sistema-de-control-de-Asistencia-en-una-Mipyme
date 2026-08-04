using FluentValidation;
using MipymeAsistencia.Application.Features.HorasExtras.Commands.RegistrarHoraExtra;

namespace MipymeAsistencia.Application.Validators;

public class RegistrarHoraExtraCommandValidator : AbstractValidator<RegistrarHoraExtraCommand>
{
    public RegistrarHoraExtraCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El id del empleado es obligatorio.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("No se pueden registrar horas extras en fechas futuras.");

        RuleFor(x => x.CantidadHoras)
            .GreaterThan(0).WithMessage("La cantidad de horas debe ser mayor a 0.")
            .LessThanOrEqualTo(12).WithMessage("No se pueden registrar más de 12 horas extras por día (Ley 185).");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede superar 500 caracteres.");

        RuleFor(x => x.FactorRecargo)
            .Must(f => f == 1.5m || f == 1.8m)
            .WithMessage("El factor de recargo debe ser 1.5 (turno diurno) o 1.8 (turno nocturno/días de descanso) según Ley 185.");
    }
}
