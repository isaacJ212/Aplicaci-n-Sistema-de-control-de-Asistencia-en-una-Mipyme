using FluentValidation;
using MipymeAsistencia.Application.Features.HorasExtras.Commands.AprobarRechazarHoraExtra;

namespace MipymeAsistencia.Application.Validators;

public class AprobarRechazarHoraExtraCommandValidator
    : AbstractValidator<AprobarRechazarHoraExtraCommand>
{
    private static readonly string[] EstadosPermitidos = ["Aprobado", "Rechazado"];

    public AprobarRechazarHoraExtraCommandValidator()
    {
        RuleFor(x => x.IdHoraExtra)
            .GreaterThan(0).WithMessage("El id de la hora extra es obligatorio.");

        RuleFor(x => x.IdUsuarioAprobador)
            .GreaterThan(0).WithMessage("El id del usuario aprobador es obligatorio.");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(e => EstadosPermitidos.Contains(e))
            .WithMessage("El estado debe ser 'Aprobado' o 'Rechazado'.");
    }
}
