namespace MipymeAsistencia.Domain.Entities;

public class HoraExtra
{
    public int IdHoraExtra { get; set; }
    public int IdEmpleado { get; set; }
    public int? IdUsuarioAprobador { get; set; }
    public DateTime Fecha { get; set; }
    public decimal CantidadHoras { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal MontoPagar { get; set; }
    public string Estado { get; set; } = "Aprobado";

    public Empleado? Empleado { get; set; }
    public Usuario? UsuarioAprobador { get; set; }
}
