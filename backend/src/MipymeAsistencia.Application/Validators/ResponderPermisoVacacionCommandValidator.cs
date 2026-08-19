using FluentValidation;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.ResponderPermisoVacacion;

namespace MipymeAsistencia.Application.Validators;

public class ResponderPermisoVacacionCommandValidator : AbstractValidator<ResponderPermisoVacacionCommand>
{
    public ResponderPermisoVacacionCommandValidator()
    {
        RuleFor(x => x.IdSolicitud)
            .GreaterThan(0).WithMessage("La solicitud es obligatoria.");

        RuleFor(x => x.IdUsuarioAprobador)
            .GreaterThan(0).WithMessage("El usuario aprobador es obligatorio.");

        RuleFor(x => x.EstadoSolicitud)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(estado => estado == "Aprobado" || estado == "Rechazado")
            .WithMessage("El estado debe ser 'Aprobado' o 'Rechazado'.");
    }
}
