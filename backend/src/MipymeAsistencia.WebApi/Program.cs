using System.Text;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.DependencyInjection;
using MipymeAsistencia.Infrastructure.DependencyInjection;
using MipymeAsistencia.WebApi.Hubs;
using MipymeAsistencia.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger con soporte JWT ───────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "MipymeAsistencia API",
        Version = "v1",
        Description = "API de control de asistencia y nómina para Mipymes (Nicaragua)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingresa el token JWT. Ejemplo: Bearer eyJhbG..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Capas de la arquitectura limpia
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── SignalR: notificaciones en tiempo real a la estación de trabajo (2FA) ─────
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificadorEstacionService, SignalREstacionService>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecureJwtSecretKeyForMipymeAsistencia123";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "MipymeAsistencia";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "MipymeAsistenciaClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode  = 401;
            ctx.Response.ContentType = "application/json";
            var body = System.Text.Json.JsonSerializer.Serialize(
                new { statusCode = 401, success = false, message = "No autorizado. Token requerido.", data = (object?)null },
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            await ctx.Response.WriteAsync(body);
        },
        OnForbidden = async ctx =>
        {
            ctx.Response.StatusCode  = 403;
            ctx.Response.ContentType = "application/json";
            var body = System.Text.Json.JsonSerializer.Serialize(
                new { statusCode = 403, success = false, message = "Acceso denegado. No tienes permisos para esta operación.", data = (object?)null },
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            await ctx.Response.WriteAsync(body);
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware global de excepciones
app.UseMiddleware<MipymeAsistencia.WebApi.Middleware.ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MipymeAsistencia API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Endpoints SignalR: estación de trabajo para recibir códigos 2FA ──────────
app.MapHub<EstacionTrabajoHub>("/hubs/estacion-trabajo");

app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "MipymeAsistencia", signalr = true }));

app.Run();
