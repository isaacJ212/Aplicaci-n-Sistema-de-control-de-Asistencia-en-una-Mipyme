namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class ActualizarDispositivoBiometricoRequestDto
{
    public string NombreDispositivo { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string TipoProtocolo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public string? ClaveComunicacion { get; set; }
    public bool Activo { get; set; }
}
