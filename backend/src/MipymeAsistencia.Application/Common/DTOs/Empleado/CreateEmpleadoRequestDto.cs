namespace MipymeAsistencia.Application.Common.DTOs.Empleado;

public class CreateEmpleadoRequestDto
{
    public int IdUsuario { get; set; }
    public string CedulaIdentificacion { get; set; } = string.Empty;
    public string NumeroInss { get; set; } = string.Empty;
    public string EstadoCivil { get; set; } = "Soltero";
    public string EstadoEmpleado { get; set; } = "Activo";
    public string? FotoUrl { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string CargoFuncion { get; set; } = string.Empty;
    public string Responsabilidades { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public decimal SalarioBaseMensual { get; set; }
    public decimal DiasVacacionesAcumuladas { get; set; } = 0m;
}
