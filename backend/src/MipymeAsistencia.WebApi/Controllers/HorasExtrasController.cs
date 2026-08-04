using System.Security.Claims;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Features.HorasExtras.Commands.AprobarRechazarHoraExtra;
using MipymeAsistencia.Application.Features.HorasExtras.Commands.RegistrarHoraExtra;
using MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasByEmpleado;
using MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasPendientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HorasExtrasController : ControllerBase
{
    private readonly IMediator _mediator;

    public HorasExtrasController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Registra horas extras para un empleado.
    /// El monto se calcula automáticamente: (SalarioBásico/240) * Factor * Horas.
    /// Accesible por Admin y Empleado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HoraExtraResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarHoraExtraRequestDto request)
    {
        var data = await _mediator.Send(new RegistrarHoraExtraCommand
        {
            IdEmpleado    = request.IdEmpleado,
            Fecha         = request.Fecha,
            CantidadHoras = request.CantidadHoras,
            Motivo        = request.Motivo,
            FactorRecargo = request.FactorRecargo
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<HoraExtraResponseDto>.Created(data,
                "Hora extra registrada correctamente. Pendiente de aprobación."));
    }

    /// <summary>
    /// Obtiene el historial de horas extras de un empleado específico.
    /// </summary>
    [HttpGet("empleado/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<HoraExtraResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmpleado(int idEmpleado)
    {
        var data = await _mediator.Send(new GetHorasExtrasByEmpleadoQuery { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<List<HoraExtraResponseDto>>.Ok(data,
            $"Se encontraron {data.Count} horas extras."));
    }

    /// <summary>
    /// Lista todas las horas extras en estado Pendiente de todos los empleados.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet("pendientes")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<HoraExtraResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendientes()
    {
        var data = await _mediator.Send(new GetHorasExtrasPendientesQuery());
        return Ok(ApiResponse<List<HoraExtraResponseDto>>.Ok(data,
            $"Se encontraron {data.Count} horas extras pendientes de aprobación."));
    }

    /// <summary>
    /// Aprueba o rechaza una hora extra pendiente.
    /// El id del usuario aprobador se resuelve desde el email del JWT.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPatch("{idHoraExtra:int}/estado")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<HoraExtraResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AprobarRechazar(
        int idHoraExtra,
        [FromBody] AprobarRechazarHoraExtraRequestDto request,
        [FromServices] MipymeAsistencia.Application.Common.Interfaces.IApplicationDbContext context)
    {
        // Resuelve el id del usuario aprobador desde el email del JWT
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        var usuario = email is not null
            ? await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email)
            : null;

        if (usuario is null)
            return Unauthorized(ApiResponse<object>.Unauthorized("No se pudo identificar al usuario aprobador."));

        var data = await _mediator.Send(new AprobarRechazarHoraExtraCommand
        {
            IdHoraExtra        = idHoraExtra,
            IdUsuarioAprobador = usuario.IdUsuario,
            Estado             = request.Estado
        });

        var mensaje = data.Estado == "Aprobado"
            ? "Hora extra aprobada correctamente."
            : "Hora extra rechazada correctamente.";

        return Ok(ApiResponse<HoraExtraResponseDto>.Ok(data, mensaje));
    }
}
