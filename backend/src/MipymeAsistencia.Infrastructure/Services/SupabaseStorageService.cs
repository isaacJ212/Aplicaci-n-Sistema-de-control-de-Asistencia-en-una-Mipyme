using System.Net.Http.Headers;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MipymeAsistencia.Infrastructure.Services;

/// <summary>
/// Implementación de IStorageService usando la REST API de Supabase Storage.
/// No requiere paquetes adicionales — usa HttpClient nativo de .NET.
///
/// Endpoint de upload:
///   POST {SupabaseUrl}/storage/v1/object/{bucket}/{fileName}
///   Headers: Authorization: Bearer {ServiceRoleKey}
///            Content-Type: {contentType}
///            x-upsert: true   ← sobreescribe si ya existe el mismo nombre
///
/// URL pública del archivo:
///   {SupabaseUrl}/storage/v1/object/public/{bucket}/{fileName}
/// </summary>
public sealed class SupabaseStorageService : IStorageService
{
    private readonly HttpClient   _http;
    private readonly string       _supabaseUrl;
    private readonly string       _serviceRoleKey;
    private readonly string       _bucketName;

    public SupabaseStorageService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _http           = factory.CreateClient("supabase");
        _supabaseUrl    = (configuration["Supabase:Url"]            ?? "").TrimEnd('/');
        _serviceRoleKey = configuration["Supabase:ServiceRoleKey"]  ?? "";
        _bucketName     = configuration["Supabase:BucketName"]      ?? "imagenes_meseta_verde";
    }

    public async Task<string> UploadAsync(string fileName, byte[] fileBytes, string contentType)
    {
        var endpoint = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{fileName}";

        using var content = new ByteArrayContent(fileBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
        request.Headers.Add("x-upsert", "true");
        request.Content = content;

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Error al subir imagen a Supabase ({response.StatusCode}): {body}");
        }

        // URL pública del objeto
        return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{fileName}";
    }
}
