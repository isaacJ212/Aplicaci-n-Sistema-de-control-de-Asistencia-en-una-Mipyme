namespace MipymeAsistencia.Domain.Entities;

/// <summary>
/// Representa un refresh token persistido en BD.
/// Permite renovar el JWT sin que el usuario vuelva a hacer login.
/// Un usuario puede tener varios tokens activos (multi-dispositivo).
/// </summary>
public class RefreshToken
{
    public int IdRefreshToken { get; set; }
    public int IdUsuario { get; set; }

    /// <summary>Token opaco de 64 bytes en Base64.</summary>
    public string Token { get; set; } = string.Empty;

    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>true cuando fue usado para generar un nuevo par de tokens.</summary>
    public bool FueUtilizado { get; set; } = false;

    /// <summary>true cuando fue revocado explícitamente por logout u otro motivo.</summary>
    public bool FueRevocado { get; set; } = false;

    // Navegación
    public Usuario? Usuario { get; set; }

    /// <summary>Indica si el token sigue siendo válido para usarse.</summary>
    public bool EsActivo => !FueRevocado && !FueUtilizado && DateTime.UtcNow < FechaExpiracion;
}
