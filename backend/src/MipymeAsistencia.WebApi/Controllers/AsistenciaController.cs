using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Features.Asistencia.Commands.GenerarQr;
using MipymeAsistencia.Application.Features.Asistencia.Commands.RegistrarAsistencia;
using MipymeAsistencia.Application.Features.Asistencia.Commands.ValidarQr;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetAllAsistencias;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetHistorialAsistencia;
using MipymeAsistencia.Application.Features.Asistencia.Queries.GetQrActual;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController : ControllerBase
{
    private readonly IMediator _mediator;

    public AsistenciaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("qr-actual")]
    [ProducesResponseType(typeof(ApiResponse<QrActualResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQrActual()
    {
        var data = await _mediator.Send(new GetQrActualQuery());
        return Ok(ApiResponse<QrActualResponseDto>.Ok(data, "QR actual obtenido correctamente."));
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAsistenciaRequestDto request)
    {
        var data = await _mediator.Send(new RegistrarAsistenciaCommand
        {
            IdEmpleado = request.IdEmpleado,
            TipoMarcaje = request.TipoMarcaje,
            LatitudMarcaje = request.LatitudMarcaje,
            LongitudMarcaje = request.LongitudMarcaje,
            TokenQrEscaneado = request.TokenQrEscaneado,
            CodigoOtpGenerado = request.CodigoOtpGenerado
        });

        return Ok(ApiResponse<AsistenciaResponseDto>.Ok(data, "Asistencia registrada correctamente."));
    }

    [HttpGet("historial/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<AsistenciaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Historial(int idEmpleado)
    {
        var data = await _mediator.Send(new GetHistorialAsistenciaQuery { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<List<AsistenciaResponseDto>>.Ok(data, "Historial de asistencia obtenido correctamente."));
    }

    /// <summary>
    /// Obtiene todos los registros de asistencia del sistema.
    /// Filtros opcionales: empleado, rango de fechas y estado.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AsistenciaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int?      idEmpleado       = null,
        [FromQuery] DateTime? fechaDesde       = null,
        [FromQuery] DateTime? fechaHasta       = null,
        [FromQuery] string?   estadoAsistencia = null)
    {
        var data = await _mediator.Send(new GetAllAsistenciasQuery
        {
            IdEmpleado       = idEmpleado,
            FechaDesde       = fechaDesde,
            FechaHasta       = fechaHasta,
            EstadoAsistencia = estadoAsistencia
        });

        return Ok(ApiResponse<List<AsistenciaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} registros de asistencia."));
    }
}
