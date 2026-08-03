namespace MipymeAsistencia.Application.Common.DTOs.Empleado;

public class UpdateEmpleadoRequestDto
{
    public string CedulaIdentificacion { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string CargoFuncion { get; set; } = string.Empty;
    public string Responsabilidades { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public decimal SalarioBaseMensual { get; set; }
    public decimal DiasVacacionesAcumuladas { get; set; }
}
