using FluentValidation;
using MipymeAsistencia.Application.Common.Behaviors;
using MipymeAsistencia.Application.Features.Auth.Commands.Login;
using Microsoft.Extensions.DependencyInjection;

namespace MipymeAsistencia.Application.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la capa Application:
    /// MediatR (CQRS handlers + pipeline behaviors) y FluentValidation.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(LoginCommand).Assembly;

        // MediatR — registra todos los handlers del assembly de Application
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);

            // ValidationBehavior intercepta cada comando/query antes del handler
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation — registra todos los validators del assembly de Application
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
