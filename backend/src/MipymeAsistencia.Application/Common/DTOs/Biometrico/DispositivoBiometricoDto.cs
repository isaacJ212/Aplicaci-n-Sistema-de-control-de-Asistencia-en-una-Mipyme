namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class DispositivoBiometricoDto
{
    public int IdDispositivo { get; set; }
    public string NombreDispositivo { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string TipoProtocolo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimaSincronizacion { get; set; }
    public string EstadoConexion { get; set; } = string.Empty;
    public int TotalMarcajesAlmacenados { get; set; }
}
