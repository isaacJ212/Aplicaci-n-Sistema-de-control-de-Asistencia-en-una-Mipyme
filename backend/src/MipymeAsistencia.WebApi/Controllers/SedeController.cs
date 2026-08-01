using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Sede;
using MipymeAsistencia.Application.Features.Sede.Commands.UpdateSede;
using MipymeAsistencia.Application.Features.Sede.Queries.GetSede;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SedeController : ControllerBase
{
    private readonly IMediator _mediator;

    public SedeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene la configuración actual de la sede.
    /// Accesible por Admin y Empleado — ambos necesitan conocer horarios y radio GPS.
    /// </summary>
    [HttpGet("configuracion")]
    [ProducesResponseType(typeof(ApiResponse<SedeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguracion()
    {
        var data = await _mediator.Send(new GetSedeQuery());
        return Ok(ApiResponse<SedeResponseDto>.Ok(data, "Configuración de sede obtenida correctamente."));
    }

    /// <summary>
    /// Actualiza la configuración de la sede (coordenadas GPS, horarios, radio).
    /// Solo el rol Admin puede modificar esta información.
    /// </summary>
    [HttpPut("configuracion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SedeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateConfiguracion([FromBody] UpdateSedeRequestDto request)
    {
        var data = await _mediator.Send(new UpdateSedeCommand
        {
            NombreSede              = request.NombreSede,
            LatitudSede             = request.LatitudSede,
            LongitudSede            = request.LongitudSede,
            RadioToleranciaMetros   = request.RadioToleranciaMetros,
            HoraEntradaOficial      = request.HoraEntradaOficial,
            HoraSalidaOficial       = request.HoraSalidaOficial,
            DuracionAlmuerzoMinutos = request.DuracionAlmuerzoMinutos
        });

        return Ok(ApiResponse<SedeResponseDto>.Ok(data, "Configuración de sede actualizada correctamente."));
    }
}
