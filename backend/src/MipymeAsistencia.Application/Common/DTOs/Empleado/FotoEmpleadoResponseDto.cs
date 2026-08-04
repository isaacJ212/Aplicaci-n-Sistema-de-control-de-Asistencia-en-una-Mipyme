namespace MipymeAsistencia.Application.Common.DTOs.Empleado;

/// <summary>
/// Respuesta al subir la foto de un empleado a Supabase Storage.
/// </summary>
public class FotoEmpleadoResponseDto
{
    public int    IdEmpleado    { get; set; }

    /// <summary>Nombre del archivo guardado en el bucket (ej. carlos_ramirez.jpg)</summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>URL pública permanente del objeto en Supabase Storage.</summary>
    public string FotoUrl       { get; set; } = string.Empty;
}
