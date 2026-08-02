namespace MipymeAsistencia.Application.Common.DTOs;

/// <summary>
/// Envuelve todas las respuestas de la API con un formato consistente:
/// código de estado HTTP, mensaje descriptivo y datos opcionales.
/// </summary>
public sealed class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // ── Fábrica de respuestas exitosas ──────────────────────────────────────

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.")
        => new() { StatusCode = 200, Success = true, Message = message, Data = data };

    public static ApiResponse<T> Created(T data, string message = "Recurso creado correctamente.")
        => new() { StatusCode = 201, Success = true, Message = message, Data = data };

    // ── Fábrica de respuestas de error ──────────────────────────────────────

    public static ApiResponse<T> BadRequest(string message)
        => new() { StatusCode = 400, Success = false, Message = message };

    public static ApiResponse<T> Unauthorized(string message)
        => new() { StatusCode = 401, Success = false, Message = message };

    public static ApiResponse<T> NotFound(string message)
        => new() { StatusCode = 404, Success = false, Message = message };

    public static ApiResponse<T> Conflict(string message)
        => new() { StatusCode = 409, Success = false, Message = message };

    public static ApiResponse<T> InternalError(string message = "Error interno del servidor.")
        => new() { StatusCode = 500, Success = false, Message = message };
}
