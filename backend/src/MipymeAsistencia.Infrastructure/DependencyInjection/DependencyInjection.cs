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

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

      
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITokenService, TokenService>();

        // HttpClient nombrado para Supabase Storage
        services.AddHttpClient("supabase");

        // Servicio de almacenamiento de imágenes en Supabase Storage
        services.AddScoped<IStorageService, SupabaseStorageService>();

        return services;
    }
}
