namespace MipymeAsistencia.Domain.Entities;

public class ValidacionQrMarcaje
{
    public int IdValidacion { get; set; }
    public int IdEmpleado { get; set; }
    public string CodigoOtpGenerado { get; set; } = string.Empty;
    public string TokenQrEscaneado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public bool FueUtilizado { get; set; }
    public int IntentosFallidos { get; set; }

    public Empleado? Empleado { get; set; }
}
