namespace MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

public class PermisoVacacionResponseDto
{
    public int IdSolicitud { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string TipoSolicitud { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal DiasSolicitados { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string EstadoSolicitud { get; set; } = string.Empty;
    public DateTime? FechaRespuesta { get; set; }
    public int? IdUsuarioAprobador { get; set; }
}
