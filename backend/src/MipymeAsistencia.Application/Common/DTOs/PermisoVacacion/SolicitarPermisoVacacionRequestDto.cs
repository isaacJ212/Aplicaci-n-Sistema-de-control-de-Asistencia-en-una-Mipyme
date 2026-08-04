namespace MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;

public class SolicitarPermisoVacacionRequestDto
{
    public int IdEmpleado { get; set; }
    public string TipoSolicitud { get; set; } = "Permiso";
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal? DiasSolicitados { get; set; }
}
