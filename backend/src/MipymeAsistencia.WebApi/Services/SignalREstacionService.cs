using Microsoft.AspNetCore.SignalR;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.WebApi.Hubs;

namespace MipymeAsistencia.WebApi.Services;

/// <summary>
/// Implementación de INotificadorEstacionService usando SignalR.
/// Envía el código 2FA a todos los clientes conectados al grupo del usuario
/// (p.e. el PC/kiosko de la estación de trabajo donde el empleado se autentica).
/// </summary>
public class SignalREstacionService : INotificadorEstacionService
{
    private readonly IHubContext<EstacionTrabajoHub> _hub;

    public SignalREstacionService(IHubContext<EstacionTrabajoHub> hub)
        => _hub = hub;

    public async Task NotificarCodigo2FaAsync(string email, string codigoPlano, DateTime expiraEn)
    {
        var grupo = $"2fa_{email.Trim().ToLowerInvariant()}";
        await _hub.Clients.Group(grupo).SendAsync("RecibirCodigo2Fa", new
        {
            email = email,
            codigo = codigoPlano,
            expiraEnUtc = expiraEn,
            generadoEnUtc = DateTime.UtcNow,
        });
    }
}
