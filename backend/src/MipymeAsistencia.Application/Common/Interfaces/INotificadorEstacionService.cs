namespace MipymeAsistencia.Application.Common.Interfaces;

public interface INotificadorEstacionService
{
    /// <summary>Notifica el código 2FA generado a la estación del usuario.</summary>
    Task NotificarCodigo2FaAsync(string email, string codigoPlano, DateTime expiraEn);
}
