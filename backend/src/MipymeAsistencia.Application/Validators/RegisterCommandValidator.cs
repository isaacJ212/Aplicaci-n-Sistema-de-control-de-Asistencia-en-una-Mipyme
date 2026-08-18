using FluentValidation;
using MipymeAsistencia.Application.Features.Auth.Commands.Register;

namespace MipymeAsistencia.Application.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private static readonly string[] RolesPermitidos = ["Admin", "Analista", "Empleado"];

    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .MaximumLength(100).WithMessage("El email no puede superar 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(100).WithMessage("La contraseña no puede superar 100 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");

        RuleFor(x => x.Role)
            .Must(r => RolesPermitidos.Contains(r))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", RolesPermitidos)}.");
    }
}
