using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Infrastructure.Persistence;
using MipymeAsistencia.Infrastructure.Persistence.UnitOfWork;
using MipymeAsistencia.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MipymeAsistencia.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // IApplicationDbContext — acceso a DbSets para queries dentro de los handlers
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // IUnitOfWork — agrupa commits atómicos; comparte la misma instancia de DbContext
        // gracias al scope, por lo que leer con IApplicationDbContext y hacer commit con
        // IUnitOfWork opera sobre la misma transacción EF Core.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
