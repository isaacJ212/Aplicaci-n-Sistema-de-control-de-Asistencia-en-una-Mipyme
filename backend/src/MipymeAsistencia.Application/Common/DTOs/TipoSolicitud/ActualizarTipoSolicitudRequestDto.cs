namespace MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;

public class ActualizarTipoSolicitudRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool DescuentaVacaciones { get; set; }
    public bool PermitePorHoras { get; set; }
    public int? MaximoDiasPorSolicitud { get; set; }
    public string? Icono { get; set; }
    public bool Activo { get; set; }
}
