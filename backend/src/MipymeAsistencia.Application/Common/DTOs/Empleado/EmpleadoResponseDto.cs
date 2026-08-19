namespace MipymeAsistencia.Application.Common.DTOs.Empleado;

public class EmpleadoResponseDto
{
    public int IdEmpleado { get; set; }
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CedulaIdentificacion { get; set; } = string.Empty;
    public string NumeroInss { get; set; } = string.Empty;
    public string EstadoCivil { get; set; } = "Soltero";
    public string EstadoEmpleado { get; set; } = "Activo";
    public string? FotoUrl { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string CargoFuncion { get; set; } = string.Empty;
    public string Departamento { get; set; } = "General";
    public string Responsabilidades { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public decimal SalarioBaseMensual { get; set; }
    public decimal DiasVacacionesAcumuladas { get; set; }
}
