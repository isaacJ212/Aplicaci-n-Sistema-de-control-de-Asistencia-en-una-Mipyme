namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class CrearDispositivoBiometricoRequestDto
{
    public string NombreDispositivo { get; set; } = "Reloj Biométrico";
    public string DireccionIp { get; set; } = "192.168.1.201";
    public int Puerto { get; set; } = 4370;
    public string TipoProtocolo { get; set; } = "ZKTeco_Standalone";
    public string? Ubicacion { get; set; } = "Entrada Principal";
    public string? ClaveComunicacion { get; set; }
    public bool Activo { get; set; } = true;
}
