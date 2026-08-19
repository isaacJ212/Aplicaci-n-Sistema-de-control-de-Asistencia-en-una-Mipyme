using System.Security.Claims;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CerrarPeriodo;
using MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CrearPeriodoCierre;
using MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.ReabrirPeriodo;
using MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodoCierreByPeriodo;
using MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Queries.GetPeriodosCierre;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PeriodoCierrePlanillaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PeriodoCierrePlanillaController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lista todos los periodos de cierre de planilla y sus fechas de corte.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PeriodoCierreDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloAbiertos)
    {
        var data = await _mediator.Send(new GetPeriodosCierreQuery { SoloAbiertos = soloAbiertos });
        return Ok(ApiResponse<List<PeriodoCierreDto>>.Ok(data, $"Se encontraron {data.Count} periodos."));
    }

    /// <summary>
    /// Obtiene el estado y fechas de corte de un periodo específico (ej. "2026-08").
    /// </summary>
    [HttpGet("{periodo}")]
    [ProducesResponseType(typeof(ApiResponse<PeriodoCierreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPeriodo(string periodo)
    {
        var data = await _mediator.Send(new GetPeriodoCierreByPeriodoQuery { Periodo = periodo });
        return Ok(ApiResponse<PeriodoCierreDto>.Ok(data, "Periodo obtenido correctamente."));
    }

    /// <summary>
    /// Crea o actualiza la configuración de fecha de corte para un periodo.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PeriodoCierreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigurarPeriodo([FromBody] CrearPeriodoCierreRequestDto request)
    {
        var data = await _mediator.Send(new CrearPeriodoCierreCommand
        {
            Periodo               = request.Periodo,
            FechaCorteHorasExtras = request.FechaCorteHorasExtras,
            FechaEmisionPlanilla  = request.FechaEmisionPlanilla,
            Observaciones         = request.Observaciones
        });

        return Ok(ApiResponse<PeriodoCierreDto>.Ok(data, $"Periodo {data.Periodo} configurado correctamente."));
    }

    /// <summary>
    /// Cierra definitivamente un periodo de planilla para evitar registros o modificaciones de horas extras.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost("{periodo}/cerrar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PeriodoCierreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarPeriodo(
        string periodo,
        [FromBody] CerrarPeriodoRequestDto? request,
        [FromServices] MipymeAsistencia.Application.Common.Interfaces.IApplicationDbContext context)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        var usuario = email != null
            ? await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email)
            : null;

        var data = await _mediator.Send(new CerrarPeriodoCommand
        {
            Periodo         = periodo,
            IdUsuarioCierre = usuario?.IdUsuario,
            Observaciones   = request?.Observaciones
        });

        return Ok(ApiResponse<PeriodoCierreDto>.Ok(data, $"Periodo {periodo} cerrado exitosamente."));
    }

    /// <summary>
    /// Reabre un periodo de planilla previamente cerrado.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost("{periodo}/reabrir")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PeriodoCierreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReabrirPeriodo(string periodo, [FromBody] CerrarPeriodoRequestDto? request)
    {
        var data = await _mediator.Send(new ReabrirPeriodoCommand
        {
            Periodo = periodo,
            Motivo  = request?.Observaciones
        });

        return Ok(ApiResponse<PeriodoCierreDto>.Ok(data, $"Periodo {periodo} reabierto exitosamente."));
    }
}
