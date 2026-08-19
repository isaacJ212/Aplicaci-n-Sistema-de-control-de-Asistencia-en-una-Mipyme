using FluentValidation;
using MipymeAsistencia.Application.Common.Behaviors;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.Features.Auth.Commands.Login;
using MipymeAsistencia.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MipymeAsistencia.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(LoginCommand).Assembly;


        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);

       
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation — registra todos los validators del assembly de Application
        services.AddValidatorsFromAssembly(applicationAssembly);

 
        services.AddSingleton<ICodigo2FaService, Codigo2FaService>();

        return services;
    }
}
