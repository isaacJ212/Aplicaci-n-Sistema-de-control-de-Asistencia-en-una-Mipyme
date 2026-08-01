namespace MipymeAsistencia.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Secret2Fa { get; set; }
    public bool Es2FaActivo { get; set; }
    public bool EstadoActivo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Rol? Rol { get; set; }
    public Empleado? Empleado { get; set; }
    public ICollection<HoraExtra> HorasExtrasAprobadas { get; set; } = new List<HoraExtra>();
    public ICollection<HistorialPermisoVacacion> PermisosAprobados { get; set; } = new List<HistorialPermisoVacacion>();
    public ICollection<EvaluacionDesempeno> EvaluacionesRealizadas { get; set; } = new List<EvaluacionDesempeno>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
