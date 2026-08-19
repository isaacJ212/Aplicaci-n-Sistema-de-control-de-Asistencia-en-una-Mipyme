using System.Collections.Concurrent;
using MipymeAsistencia.Application.Common.Interfaces;

namespace MipymeAsistencia.Application.Services;

public class Codigo2FaService : ICodigo2FaService
{
    private readonly ConcurrentDictionary<string, (string Codigo, DateTime Expiracion)> _cache = new();

    private static readonly TimeSpan DuracionDefault = TimeSpan.FromMinutes(5);

    public void Guardar(string email, string codigoPlano, TimeSpan? duracion = null)
    {
        var expiracion = DateTime.UtcNow.Add(duracion ?? DuracionDefault);
        _cache[email.Normalize().Trim().ToLowerInvariant()] = (codigoPlano, expiracion);
    }

    public string? ObtenerUltimo(string email)
    {
        var key = email.Normalize().Trim().ToLowerInvariant();

        if (_cache.TryGetValue(key, out var entrada))
        {
            if (entrada.Expiracion > DateTime.UtcNow)
                return entrada.Codigo;

            _cache.TryRemove(key, out _);
        }
        return null;
    }

    public void Invalidar(string email)
    {
        var key = email.Normalize().Trim().ToLowerInvariant();
        _cache.TryRemove(key, out _);
    }
}
