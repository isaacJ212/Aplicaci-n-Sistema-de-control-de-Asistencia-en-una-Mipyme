using System.Security.Claims;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.PermisoVacacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.ResponderPermisoVacacion;
using MipymeAsistencia.Application.Features.PermisoVacacion.Commands.SolicitarPermisoVacacion;
using MipymeAsistencia.Application.Features.PermisoVacacion.Queries.GetSolicitudesPermisoVacacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermisoVacacionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public PermisoVacacionController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context  = context;
    }


    private async Task<int?> ObtenerIdUsuarioDelJwt(CancellationToken ct = default)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(email)) return null;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        return usuario?.IdUsuario;
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
            IdEmpleado       = request.IdEmpleado,
            TipoSolicitud    = request.TipoSolicitud,
            FechaInicio      = request.FechaInicio,
            FechaFin         = request.FechaFin,
            Motivo           = request.Motivo,
            DiasSolicitados  = request.DiasSolicitados,
            HorasSolicitadas = request.HorasSolicitadas
        });

        string msg = data.UnidadTiempo == "Horas"
            ? $"Solicitud enviada correctamente ({data.HorasSolicitadas} horas)."
            : "Solicitud enviada correctamente.";

        return Ok(ApiResponse<PermisoVacacionResponseDto>.Ok(data, msg));
    }

    [HttpGet("solicitudes")]
    [Authorize(Roles = "Admin,Analista")]
    [ProducesResponseType(typeof(ApiResponse<List<PermisoVacacionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSolicitudes(
        [FromQuery] int?    idEmpleado      = null,
        [FromQuery] string? estadoSolicitud = null,
        [FromQuery] string? tipoSolicitud   = null)
    {
        var data = await _mediator.Send(new GetSolicitudesPermisoVacacionQuery
        {
            IdEmpleado      = idEmpleado,
            EstadoSolicitud = estadoSolicitud,
            TipoSolicitud   = tipoSolicitud
        });

        return Ok(ApiResponse<List<PermisoVacacionResponseDto>>.Ok(data, "Solicitudes obtenidas correctamente."));
    }

 
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
    [Authorize(Roles = "Admin,Analista")]
    [ProducesResponseType(typeof(ApiResponse<PermisoVacacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Responder(
        int idSolicitud,
        [FromBody] ResponderPermisoVacacionRequestDto request,
        CancellationToken cancellationToken)
    {
        // Resolver el aprobador desde el JWT — no se requiere en el body
        var idUsuarioAprobador = await ObtenerIdUsuarioDelJwt(cancellationToken);
        if (!idUsuarioAprobador.HasValue)
            return Unauthorized(ApiResponse<object>.Unauthorized(
                "No se pudo identificar al usuario aprobador desde el token."));

        var data = await _mediator.Send(new ResponderPermisoVacacionCommand
        {
            IdSolicitud        = idSolicitud,
            IdUsuarioAprobador = idUsuarioAprobador.Value,
            EstadoSolicitud    = request.EstadoSolicitud
        });

        return Ok(ApiResponse<PermisoVacacionResponseDto>.Ok(data, "Solicitud respondida correctamente."));
    }
}
