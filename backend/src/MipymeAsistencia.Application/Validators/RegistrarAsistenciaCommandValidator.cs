using FluentValidation;
using MipymeAsistencia.Application.Features.Asistencia.Commands.RegistrarAsistencia;

namespace MipymeAsistencia.Application.Validators;

public class RegistrarAsistenciaCommandValidator : AbstractValidator<RegistrarAsistenciaCommand>
{
    public RegistrarAsistenciaCommandValidator()
    {
        RuleFor(x => x.IdEmpleado)
            .GreaterThan(0).WithMessage("El id del empleado es obligatorio.");

        RuleFor(x => x.TipoMarcaje)
            .NotEmpty().WithMessage("El tipo de marcaje es obligatorio.")
            .Must(t => t == "Entrada" || t == "Salida" || t == "InicioAlmuerzo" || t == "FinAlmuerzo")
            .WithMessage("El tipo de marcaje no es válido.");

        RuleFor(x => x.LatitudMarcaje)
            .InclusiveBetween(-90m, 90m).WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.LongitudMarcaje)
            .InclusiveBetween(-180m, 180m).WithMessage("La longitud debe estar entre -180 y 180.");

        RuleFor(x => x.TokenQrEscaneado)
            .NotEmpty().WithMessage("El token QR es obligatorio.");

        RuleFor(x => x.CodigoOtpGenerado)
            .NotEmpty().WithMessage("El código OTP es obligatorio.");
    }
}
