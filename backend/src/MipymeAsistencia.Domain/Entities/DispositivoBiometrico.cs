namespace MipymeAsistencia.Domain.Entities;

public class DispositivoBiometrico
{
    public int IdDispositivo { get; set; }
    public string NombreDispositivo { get; set; } = "Reloj Biométrico";
    public string DireccionIp { get; set; } = "192.168.1.201";
    public int Puerto { get; set; } = 4370; // Puerto estándar ZKTeco
    public string TipoProtocolo { get; set; } = "ZKTeco_Standalone"; // ZKTeco_Standalone, Hikvision_ISAPI, Virtual_Mock
    public string? Ubicacion { get; set; } = "Entrada Principal";
    public string? ClaveComunicacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimaSincronizacion { get; set; }
    public string EstadoConexion { get; set; } = "Desconectado"; // Conectado, Desconectado, Sincronizado, Error

    public ICollection<RegistroMarcajeBiometrico> RegistrosMarcajes { get; set; } = new List<RegistroMarcajeBiometrico>();
}
