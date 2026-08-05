using FluentValidation;
using MipymeAsistencia.Application.Features.Sede.Commands.UpdateSede;

namespace MipymeAsistencia.Application.Validators;

public class UpdateSedeCommandValidator : AbstractValidator<UpdateSedeCommand>
{
    public UpdateSedeCommandValidator()
    {
        RuleFor(x => x.NombreSede)
            .NotEmpty().WithMessage("El nombre de la sede es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.LatitudSede)
            .InclusiveBetween(-90m, 90m)
            .WithMessage("La latitud debe estar entre -90 y 90 grados.");

        RuleFor(x => x.LongitudSede)
            .InclusiveBetween(-180m, 180m)
            .WithMessage("La longitud debe estar entre -180 y 180 grados.");

        RuleFor(x => x.RadioToleranciaMetros)
            .GreaterThan(0).WithMessage("El radio de tolerancia debe ser mayor a 0 metros.")
            .LessThanOrEqualTo(5000).WithMessage("El radio de tolerancia no puede superar 5000 metros.");

        RuleFor(x => x.HoraEntradaOficial)
            .NotEmpty().WithMessage("La hora de entrada es obligatoria.")
            .Matches(@"^\d{2}:\d{2}$").WithMessage("La hora de entrada debe tener el formato HH:mm.")
            .Must(BeValidTime).WithMessage("La hora de entrada no es una hora válida.");

        RuleFor(x => x.HoraSalidaOficial)
            .NotEmpty().WithMessage("La hora de salida es obligatoria.")
            .Matches(@"^\d{2}:\d{2}$").WithMessage("La hora de salida debe tener el formato HH:mm.")
            .Must(BeValidTime).WithMessage("La hora de salida no es una hora válida.");

        RuleFor(x => x)
            .Must(x => BeValidTime(x.HoraEntradaOficial) &&
                       BeValidTime(x.HoraSalidaOficial)  &&
                       TimeSpan.Parse(x.HoraEntradaOficial) < TimeSpan.Parse(x.HoraSalidaOficial))
            .WithMessage("La hora de entrada debe ser anterior a la hora de salida.")
            .When(x => BeValidTime(x.HoraEntradaOficial) && BeValidTime(x.HoraSalidaOficial));

        RuleFor(x => x.DuracionAlmuerzoMinutos)
            .GreaterThan(0).WithMessage("La duración del almuerzo debe ser mayor a 0 minutos.")
            .LessThanOrEqualTo(180).WithMessage("La duración del almuerzo no puede superar 180 minutos.");

        RuleFor(x => x.MinutosTolerancia)
            .GreaterThanOrEqualTo(0).WithMessage("Los minutos de tolerancia no pueden ser negativos.")
            .LessThanOrEqualTo(60).WithMessage("La tolerancia no puede superar 60 minutos.");
    }

    private static bool BeValidTime(string value)
        => !string.IsNullOrWhiteSpace(value) && TimeSpan.TryParse(value, out _);
}
