using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Features.DiasFeriados.Commands.ActualizarDiaFeriado;
using MipymeAsistencia.Application.Features.DiasFeriados.Commands.CrearDiaFeriado;
using MipymeAsistencia.Application.Features.DiasFeriados.Commands.EliminarDiaFeriado;
using MipymeAsistencia.Application.Features.DiasFeriados.Queries.EsDiaFeriado;
using MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiaFeriadoById;
using MipymeAsistencia.Application.Features.DiasFeriados.Queries.GetDiasFeriados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiaFeriadoController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiaFeriadoController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene la lista de todos los días feriados registrados. Opcionalmente filtra por año.
    /// Accesible por Admin y Empleados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DiaFeriadoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? anio)
    {
        var data = await _mediator.Send(new GetDiasFeriadosQuery { Anio = anio });
        return Ok(ApiResponse<List<DiaFeriadoDto>>.Ok(data, $"Se encontraron {data.Count} días feriados."));
    }

    /// <summary>
    /// Obtiene el detalle de un día feriado por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DiaFeriadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _mediator.Send(new GetDiaFeriadoByIdQuery { IdDiaFeriado = id });
        return Ok(ApiResponse<DiaFeriadoDto>.Ok(data, "Día feriado obtenido correctamente."));
    }

    /// <summary>
    /// Consulta si una fecha específica (YYYY-MM-DD) es día feriado.
    /// </summary>
    [HttpGet("es-feriado")]
    [ProducesResponseType(typeof(ApiResponse<DiaFeriadoDto?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EsFeriado([FromQuery] DateTime fecha)
    {
        var data = await _mediator.Send(new EsDiaFeriadoQuery { Fecha = fecha });
        var mensaje = data != null
            ? $"La fecha {fecha:yyyy-MM-dd} es feriado: {data.Nombre}."
            : $"La fecha {fecha:yyyy-MM-dd} no es un día feriado.";
        return Ok(ApiResponse<DiaFeriadoDto?>.Ok(data, mensaje));
    }

    /// <summary>
    /// Crea un nuevo día feriado en el sistema.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DiaFeriadoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearDiaFeriadoRequestDto request)
    {
        var data = await _mediator.Send(new CrearDiaFeriadoCommand
        {
            Fecha         = request.Fecha,
            Nombre        = request.Nombre,
            Descripcion   = request.Descripcion,
            EsRecuperable = request.EsRecuperable,
            EsMovil       = request.EsMovil
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<DiaFeriadoDto>.Created(data, "Día feriado registrado correctamente."));
    }

    /// <summary>
    /// Actualiza un día feriado existente.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DiaFeriadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarDiaFeriadoRequestDto request)
    {
        var data = await _mediator.Send(new ActualizarDiaFeriadoCommand
        {
            IdDiaFeriado  = id,
            Fecha         = request.Fecha,
            Nombre        = request.Nombre,
            Descripcion   = request.Descripcion,
            EsRecuperable = request.EsRecuperable,
            EsMovil       = request.EsMovil
        });

        return Ok(ApiResponse<DiaFeriadoDto>.Ok(data, "Día feriado actualizado correctamente."));
    }

    /// <summary>
    /// Elimina un día feriado del sistema.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _mediator.Send(new EliminarDiaFeriadoCommand { IdDiaFeriado = id });
        return Ok(ApiResponse<bool>.Ok(result, "Día feriado eliminado correctamente."));
    }
}
