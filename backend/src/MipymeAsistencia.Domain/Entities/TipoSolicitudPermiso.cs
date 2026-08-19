namespace MipymeAsistencia.Domain.Entities;

public class TipoSolicitudPermiso
{
    public int IdTipoSolicitud { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereComprobante { get; set; } = false;
    public bool DescuentaVacaciones { get; set; } = false;
    public bool PermitePorHoras { get; set; } = true;
    public int? MaximoDiasPorSolicitud { get; set; }
    public string? Icono { get; set; } = "calendar";
    public bool Activo { get; set; } = true;
}
