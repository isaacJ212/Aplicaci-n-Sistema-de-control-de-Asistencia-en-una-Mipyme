namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class RegistroMarcajeBiometricoDto
{
    public int IdRegistroBiometrico { get; set; }
    public int IdDispositivo { get; set; }
    public string NombreDispositivo { get; set; } = string.Empty;
    public string NumeroEnrollamiento { get; set; } = string.Empty;
    public string? NombreEmpleado { get; set; }
    public DateTime FechaHora { get; set; }
    public string FechaHoraFormato => FechaHora.ToString("yyyy-MM-dd HH:mm:ss");
    public int TipoMarcaje { get; set; }
    public string TipoMarcajeDescripcion => TipoMarcaje switch
    {
        0 => "Entrada",
        1 => "Salida",
        2 => "Inicio Almuerzo",
        3 => "Fin Almuerzo",
        _ => "General"
    };
    public string TipoVerificacion { get; set; } = string.Empty;
    public bool Procesado { get; set; }
    public DateTime? FechaProcesado { get; set; }
    public int? IdAsistenciaGenerada { get; set; }
    public string? ErrorProcesamiento { get; set; }
}
