using MipymeAsistencia.Application.Features.Auth.Commands.Login;
using MipymeAsistencia.Application.Features.Auth.Commands.Register;
using Microsoft.Extensions.DependencyInjection;

namespace MipymeAsistencia.Application.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la capa Application:
    /// MediatR (CQRS handlers) y FluentValidation.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra todos los IRequestHandler<,> del assembly de Application
        // usando el assembly de cualquier comando ya existente como ancla.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(LoginCommand).Assembly,
                typeof(RegisterCommand).Assembly
            );
        });

        return services;
    }
}
