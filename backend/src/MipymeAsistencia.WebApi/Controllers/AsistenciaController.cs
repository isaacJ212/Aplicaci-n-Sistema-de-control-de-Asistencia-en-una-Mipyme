using System.Security.Claims;
using System.Security.Cryptography;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.Features.Asistencia.Commands.GenerarQr;
using MipymeAsistencia.Application.Features.Asistencia.Commands.RegistrarAsistencia;
using MipymeAsistencia.Application.Features.Asistencia.Commands.ValidarQr;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetAlertasTardanza;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetAllAsistencias;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetHistorialAsistencia;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetInformeAsistencia;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetQrActual;
using MipymeAsistencia.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public AsistenciaController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    private async Task<int?> ObtenerIdEmpleadoDelJwt(CancellationToken ct = default)
    {
        // 1. Intentar desde claim "idEmpleado" (si en el futuro se agrega al JWT)
        var claim = User.Claims.FirstOrDefault(c => c.Type == "idEmpleado");
        if (claim is not null && int.TryParse(claim.Value, out var id))
            return id;

        // 2. Resolver desde el email del JWT (siempre disponible)
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(email)) return null;

        var empleado = await _context.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.Usuario != null && e.Usuario.Email == email, ct);

        return empleado?.IdEmpleado;
    }

    [HttpGet("qr-actual")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<QrActualResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQrActual()
    {
        var data = await _mediator.Send(new GetQrActualQuery());
        return Ok(ApiResponse<QrActualResponseDto>.Ok(data, "QR actual obtenido correctamente."));
    }

    [HttpPost("rotar-qr-sede")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<QrActualResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RotarQrSede()
    {
        var sede = await _context.ConfiguracionesSede.FirstOrDefaultAsync();
        if (sede is null)
        {
            sede = new ConfiguracionSede
            {
                NombreSede = "Sede Principal",
                LatitudSede = 12.13500m,
                LongitudSede = -86.28000m,
                RadioToleranciaMetros = 200,
                HoraEntradaOficial = new TimeSpan(8, 0, 0),
                HoraSalidaOficial = new TimeSpan(17, 0, 0),
                DuracionAlmuerzoMinutos = 60,
                MinutosTolerancia = 10,
            };
            _context.ConfiguracionesSede.Add(sede);
        }

        sede.TokenQrActual = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        sede.QrUltimaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var dto = new QrActualResponseDto
        {
            IdSede = sede.IdSede,
            NombreSede = sede.NombreSede,
            TokenQrActual = sede.TokenQrActual,
            QrUltimaActualizacion = sede.QrUltimaActualizacion,
            RadioToleranciaMetros = sede.RadioToleranciaMetros,
        };
        return Ok(ApiResponse<QrActualResponseDto>.Ok(dto, "QR de sede renovado correctamente."));
    }

    [HttpPost("generar-qr/{idEmpleado:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarQr(int idEmpleado)
    {
        var token = await _mediator.Send(new GenerarQrCommand { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<string>.Ok(token, "QR generado correctamente."));
    }

    [HttpPost("validar-qr")]
    [ProducesResponseType(typeof(ApiResponse<ValidarQrResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidarQr([FromBody] ValidarQrRequestDto request)
    {
        var data = await _mediator.Send(new ValidarQrCommand
        {
            IdEmpleado = request.IdEmpleado,
            TokenQrEscaneado = request.TokenQrEscaneado
        });

        return Ok(ApiResponse<ValidarQrResponseDto>.Ok(data, "QR validado correctamente."));
    }

    [HttpPost("registrar")]
    [ProducesResponseType(typeof(ApiResponse<AsistenciaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarAsistenciaRequestDto request,
        CancellationToken cancellationToken)
    {
        // Resuelve idEmpleado desde el JWT (email → empleado)
        var idEmpleado = await ObtenerIdEmpleadoDelJwt(cancellationToken);
        if (!idEmpleado.HasValue)
            return BadRequest(ApiResponse<object>.BadRequest(
                "Tu usuario no tiene un expediente de empleado asociado. Contacta al administrador."));

        var data = await _mediator.Send(new RegistrarAsistenciaCommand
        {
            IdEmpleado        = idEmpleado.Value,
            TipoMarcaje       = request.TipoMarcaje,
            LatitudMarcaje    = request.LatitudMarcaje,
            LongitudMarcaje   = request.LongitudMarcaje,
            TokenQrEscaneado  = request.TokenQrEscaneado,
            CodigoOtpGenerado = request.CodigoOtpGenerado
        });

        return Ok(ApiResponse<AsistenciaResponseDto>.Ok(data, data.Mensaje ?? "Asistencia registrada correctamente."));
    }

    [HttpGet("historial/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<AsistenciaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Historial(int idEmpleado)
    {
        var data = await _mediator.Send(new GetHistorialAsistenciaQuery { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<List<AsistenciaResponseDto>>.Ok(data, "Historial de asistencia obtenido correctamente."));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AsistenciaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? idEmpleado = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] string? estadoAsistencia = null)
    {
        var data = await _mediator.Send(new GetAllAsistenciasQuery
        {
            IdEmpleado = idEmpleado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            EstadoAsistencia = estadoAsistencia
        });

        return Ok(ApiResponse<List<AsistenciaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} registros de asistencia."));
    }

    [HttpGet("alertas-tardanza")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AlertaTardanzaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasTardanza(
        [FromQuery] string? periodo = null,
        [FromQuery] int umbral = 3)
    {
        var data = await _mediator.Send(new GetAlertasTardanzaQuery
        {
            PeriodoMesAnio = periodo ?? string.Empty,
            UmbralReincidencia = umbral,
        });

        var reincidentes = data.Count(a => a.EsReincidente);
        return Ok(ApiResponse<List<AlertaTardanzaDto>>.Ok(
            data,
            $"{data.Count} empleado(s) con tardanzas · {reincidentes} reincidente(s)."));
    }

    
    [HttpGet("informe")]
    [Authorize(Roles = "Admin,Analista")]
    [ProducesResponseType(typeof(ApiResponse<List<InformeAsistenciaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInforme(
        [FromQuery] int?      idEmpleado  = null,
        [FromQuery] DateTime? fechaDesde  = null,
        [FromQuery] DateTime? fechaHasta  = null)
    {
        var hoy    = DateTime.UtcNow.Date;
        var desde  = fechaDesde?.Date  ?? new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta  = fechaHasta?.Date  ?? hoy;

        if (desde > hasta)
            return BadRequest(ApiResponse<object>.BadRequest(
                "La fecha de inicio no puede ser posterior a la fecha final."));

        var data = await _mediator.Send(new GetInformeAsistenciaQuery
        {
            IdEmpleado = idEmpleado,
            FechaDesde = desde,
            FechaHasta = hasta,
        });

        return Ok(ApiResponse<List<InformeAsistenciaDto>>.Ok(
            data, $"Informe generado para {data.Count} empleado(s) · {desde:dd/MM/yyyy} – {hasta:dd/MM/yyyy}."));
    }
}
