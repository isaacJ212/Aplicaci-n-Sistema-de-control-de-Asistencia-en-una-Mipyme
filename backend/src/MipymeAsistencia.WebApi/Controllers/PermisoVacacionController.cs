using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.ResponderPermisoVacacion;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.SolicitarPermisoVacacion;
using MipymeAsistencia.Application.Features.PermisoVacacion.Queries.GetSolicitudesPermisoVacacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermisoVacacionController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermisoVacacionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("solicitar")]
    [Authorize(Roles = "Empleado")]
    [ProducesResponseType(typeof(ApiResponse<PermisoVacacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Solicitar([FromBody] SolicitarPermisoVacacionRequestDto request)
    {
        var data = await _mediator.Send(new SolicitarPermisoVacacionCommand
        {
            IdEmpleado = request.IdEmpleado,
            TipoSolicitud = request.TipoSolicitud,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Motivo = request.Motivo,
            DiasSolicitados = request.DiasSolicitados
        });

        return Ok(ApiResponse<PermisoVacacionResponseDto>.Ok(data, "Solicitud enviada correctamente."));
    }

    [HttpGet("solicitudes")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PermisoVacacionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSolicitudes([FromQuery] int? idEmpleado, [FromQuery] string? estadoSolicitud, [FromQuery] string? tipoSolicitud)
    {
        var data = await _mediator.Send(new GetSolicitudesPermisoVacacionQuery
        {
            IdEmpleado = idEmpleado,
            EstadoSolicitud = estadoSolicitud,
            TipoSolicitud = tipoSolicitud
        });

        return Ok(ApiResponse<List<PermisoVacacionResponseDto>>.Ok(data, "Solicitudes obtenidas correctamente."));
    }

    /// <summary>
    /// Obtiene las solicitudes de permiso/vacación de un empleado específico.
    /// Accesible por el empleado para ver su propio historial, y por Admin para ver el de cualquiera.
    /// Filtros opcionales: estado y tipo de solicitud.
    /// </summary>
    [HttpGet("mis-solicitudes/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<PermisoVacacionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMisSolicitudes(
        int idEmpleado,
        [FromQuery] string? estadoSolicitud = null,
        [FromQuery] string? tipoSolicitud   = null)
    {
        var data = await _mediator.Send(new GetSolicitudesPermisoVacacionQuery
        {
            IdEmpleado      = idEmpleado,
            EstadoSolicitud = estadoSolicitud,
            TipoSolicitud   = tipoSolicitud
        });

        return Ok(ApiResponse<List<PermisoVacacionResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} solicitudes."));
    }

    [HttpPut("{idSolicitud:int}/responder")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PermisoVacacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Responder(int idSolicitud, [FromBody] ResponderPermisoVacacionRequestDto request)
    {
        var data = await _mediator.Send(new ResponderPermisoVacacionCommand
        {
            IdSolicitud = idSolicitud,
            IdUsuarioAprobador = request.IdUsuarioAprobador,
            EstadoSolicitud = request.EstadoSolicitud
        });

        return Ok(ApiResponse<PermisoVacacionResponseDto>.Ok(data, "Solicitud respondida correctamente."));
    }
}
