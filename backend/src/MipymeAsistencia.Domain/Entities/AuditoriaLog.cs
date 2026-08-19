namespace MipymeAsistencia.Domain.Entities;

public class AuditoriaLog
{
    public int      IdLog         { get; set; }
    public string   Entidad       { get; set; } = string.Empty;
    public int      IdRegistro    { get; set; }
    public string   Accion        { get; set; } = string.Empty;
    public string   Usuario       { get; set; } = string.Empty;
    public string   Descripcion   { get; set; } = string.Empty;
    public string?  DetallesJson  { get; set; }
    public DateTime FechaEvento   { get; set; } = DateTime.UtcNow;
}
