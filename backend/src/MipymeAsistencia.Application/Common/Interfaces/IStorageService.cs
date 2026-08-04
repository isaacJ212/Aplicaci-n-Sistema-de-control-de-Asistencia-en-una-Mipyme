namespace MipymeAsistencia.Application.Common.Interfaces;

/// <summary>
/// Contrato para subir archivos a un servicio de almacenamiento externo.
/// La implementación concreta usa Supabase Storage REST API.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Sube un archivo al bucket configurado y retorna la URL pública del objeto.
    /// </summary>
    /// <param name="fileName">Nombre con el que se guardará el archivo (incluye extensión).</param>
    /// <param name="fileBytes">Contenido binario del archivo.</param>
    /// <param name="contentType">MIME type, ej. "image/jpeg".</param>
    /// <returns>URL pública permanente del archivo subido.</returns>
    Task<string> UploadAsync(string fileName, byte[] fileBytes, string contentType);
}
