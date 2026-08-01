namespace MipymeAsistencia.Domain.Entities;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public int IdUsuario { get; set; }
    public string CedulaIdentificacion { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string CargoFuncion { get; set; } = string.Empty;
    public string Responsabilidades { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public decimal SalarioBaseMensual { get; set; }
    public decimal DiasVacacionesAcumuladas { get; set; } = 0m;

    public Usuario? Usuario { get; set; }
    public ICollection<ValidacionQrMarcaje> ValidacionesQrMarcaje { get; set; } = new List<ValidacionQrMarcaje>();
    public ICollection<HistorialAsistencia> HistorialAsistencias { get; set; } = new List<HistorialAsistencia>();
    public ICollection<HoraExtra> HorasExtras { get; set; } = new List<HoraExtra>();
    public ICollection<HistorialPermisoVacacion> Solicitudes { get; set; } = new List<HistorialPermisoVacacion>();
    public ICollection<HistorialPlanilla> Planillas { get; set; } = new List<HistorialPlanilla>();
    public ICollection<EvaluacionDesempeno> Evaluaciones { get; set; } = new List<EvaluacionDesempeno>();
}
