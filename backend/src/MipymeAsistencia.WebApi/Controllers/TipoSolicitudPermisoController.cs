using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.TipoSolicitud;
using MipymeAsistencia.Application.Features.TipoSolicitud.Commands.ActualizarTipoSolicitud;
using MipymeAsistencia.Application.Features.TipoSolicitud.Commands.CrearTipoSolicitud;
using MipymeAsistencia.Application.Features.TipoSolicitud.Commands.EliminarTipoSolicitud;
using MipymeAsistencia.Application.Features.TipoSolicitud.Queries.GetTipoSolicitudById;
using MipymeAsistencia.Application.Features.TipoSolicitud.Queries.GetTiposSolicitud;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TipoSolicitudPermisoController : ControllerBase
{
    private readonly IMediator _mediator;

    public TipoSolicitudPermisoController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lista los tipos de solicitud de permisos y ausencias configurados en el sistema.
    /// Accesible por cualquier usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TipoSolicitudPermisoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos = true)
    {
        var data = await _mediator.Send(new GetTiposSolicitudQuery { SoloActivos = soloActivos });
        return Ok(ApiResponse<List<TipoSolicitudPermisoDto>>.Ok(data, $"Se encontraron {data.Count} tipos de solicitud."));
    }

    /// <summary>
    /// Obtiene el detalle de un tipo de solicitud por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TipoSolicitudPermisoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _mediator.Send(new GetTipoSolicitudByIdQuery { IdTipoSolicitud = id });
        return Ok(ApiResponse<TipoSolicitudPermisoDto>.Ok(data, "Tipo de solicitud obtenido correctamente."));
    }

    /// <summary>
    /// Crea un nuevo tipo de solicitud de permiso/ausencia.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TipoSolicitudPermisoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearTipoSolicitudRequestDto request)
    {
        var data = await _mediator.Send(new CrearTipoSolicitudCommand
        {
            Nombre                 = request.Nombre,
            Descripcion            = request.Descripcion,
            RequiereComprobante    = request.RequiereComprobante,
            DescuentaVacaciones    = request.DescuentaVacaciones,
            PermitePorHoras        = request.PermitePorHoras,
            MaximoDiasPorSolicitud = request.MaximoDiasPorSolicitud,
            Icono                  = request.Icono,
            Activo                 = request.Activo
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<TipoSolicitudPermisoDto>.Created(data, "Tipo de solicitud creado exitosamente."));
    }

    /// <summary>
    /// Actualiza un tipo de solicitud existente.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TipoSolicitudPermisoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTipoSolicitudRequestDto request)
    {
        var data = await _mediator.Send(new ActualizarTipoSolicitudCommand
        {
            IdTipoSolicitud        = id,
            Nombre                 = request.Nombre,
            Descripcion            = request.Descripcion,
            RequiereComprobante    = request.RequiereComprobante,
            DescuentaVacaciones    = request.DescuentaVacaciones,
            PermitePorHoras        = request.PermitePorHoras,
            MaximoDiasPorSolicitud = request.MaximoDiasPorSolicitud,
            Icono                  = request.Icono,
            Activo                 = request.Activo
        });

        return Ok(ApiResponse<TipoSolicitudPermisoDto>.Ok(data, "Tipo de solicitud actualizado exitosamente."));
    }

    /// <summary>
    /// Elimina un tipo de solicitud de permiso.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _mediator.Send(new EliminarTipoSolicitudCommand { IdTipoSolicitud = id });
        return Ok(ApiResponse<bool>.Ok(result, "Tipo de solicitud eliminado exitosamente."));
    }
}
