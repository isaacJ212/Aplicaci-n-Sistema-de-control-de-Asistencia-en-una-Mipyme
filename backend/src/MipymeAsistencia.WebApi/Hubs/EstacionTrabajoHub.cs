using Microsoft.AspNetCore.SignalR;

namespace MipymeAsistencia.WebApi.Hubs;



public class EstacionTrabajoHub : Hub
{
 
    public async Task SuscribirseAUsuario(string email)
    {
        var grupo = GrupoDe(email);
        await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
    }

    public async Task DesuscribirseDeUsuario(string email)
    {
        var grupo = GrupoDe(email);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
    }

    private static string GrupoDe(string email)
        => $"2fa_{email.Trim().ToLowerInvariant()}";
}
