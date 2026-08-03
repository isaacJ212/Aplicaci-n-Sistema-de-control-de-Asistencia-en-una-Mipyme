using FluentValidation;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.SolicitarPermisoVacacion;

namespace MipymeAsistencia.Application.Validators;

public class SolicitarPermisoVacacionCommandValidator : AbstractValidator<SolicitarPermisoVacacionCommand>
{
    public SolicitarPermisoVacacionCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El empleado es obligatorio.");

        RuleFor(x => x.TipoSolicitud)
            .NotEmpty().WithMessage("El tipo de solicitud es obligatorio.")
            .Must(tipo => tipo == "Permiso" || tipo == "Vacacion")
            .WithMessage("El tipo de solicitud debe ser 'Permiso' o 'Vacacion'.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.FechaFin)
            .NotEmpty().WithMessage("La fecha final es obligatoria.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(x => x)
            .Must(x => x.FechaInicio <= x.FechaFin)
            .WithMessage("La fecha de inicio no puede ser mayor que la fecha final.");
    }
}
