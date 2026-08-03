using FluentValidation;
using MipymeAsistencia.Application.Common.DTOs;
using System.Net;
using System.Text.Json;

namespace MipymeAsistencia.WebApi.Middleware;

/// <summary>
/// Middleware global que intercepta todas las excepciones no controladas.
/// Elimina la necesidad de try/catch en cada controlador.
/// Mapea cada tipo de excepción a su código HTTP correspondiente.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // FluentValidation — 422 Unprocessable Entity con todos los mensajes
            var errores = ex.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            var response = new ApiResponse<object>
            {
                StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                Success    = false,
                Message    = "Errores de validación.",
                Data       = new { errores }
            };

            await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            var response = ApiResponse<object>.Unauthorized(ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized, response);
        }
        catch (InvalidOperationException ex)
        {
            var response = ApiResponse<object>.Conflict(ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.Conflict, response);
        }
        catch (KeyNotFoundException ex)
        {
            var response = new ApiResponse<object>
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Success    = false,
                Message    = ex.Message
            };
            await WriteResponseAsync(context, HttpStatusCode.NotFound, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            var response = ApiResponse<object>.InternalError();
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, response);
        }
    }

    private static async Task WriteResponseAsync<T>(
        HttpContext context,
        HttpStatusCode statusCode,
        ApiResponse<T> body)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
