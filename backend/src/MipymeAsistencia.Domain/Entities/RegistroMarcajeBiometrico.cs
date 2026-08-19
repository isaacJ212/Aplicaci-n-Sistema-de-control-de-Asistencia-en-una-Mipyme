namespace MipymeAsistencia.Domain.Entities;

public class RegistroMarcajeBiometrico
{
    public int IdRegistroBiometrico { get; set; }
    public int IdDispositivo { get; set; }
    public string NumeroEnrollamiento { get; set; } = string.Empty; // Mapea a Cédula, INSS o IdEmpleado
    public DateTime FechaHora { get; set; }
    public int TipoMarcaje { get; set; } = 0; // 0: Entrada, 1: Salida, 2: Inicio Almuerzo, 3: Fin Almuerzo, 4: Indefinido
    public string TipoVerificacion { get; set; } = "Huella"; // Huella, Rostro, Tarjeta, PIN, QR
    public bool Procesado { get; set; } = false;
    public DateTime? FechaProcesado { get; set; }
    public int? IdAsistenciaGenerada { get; set; }
    public string? ErrorProcesamiento { get; set; }

    public DispositivoBiometrico? Dispositivo { get; set; }
    public HistorialAsistencia? AsistenciaGenerada { get; set; }
}
