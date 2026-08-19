namespace MipymeAsistencia.Domain.Entities;

public class HistorialPermisoVacacion
{
    public int IdSolicitud { get; set; }
    public int IdEmpleado { get; set; }
    public int? IdUsuarioAprobador { get; set; }
    public string TipoSolicitud { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal DiasSolicitados { get; set; }
    public decimal? HorasSolicitadas { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string EstadoSolicitud { get; set; } = "Pendiente";
    public DateTime? FechaRespuesta { get; set; }

    public Empleado? Empleado { get; set; }
    public Usuario? UsuarioAprobador { get; set; }
}
