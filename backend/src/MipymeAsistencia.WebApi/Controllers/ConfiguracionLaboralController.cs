using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.ActualizarTablaIr;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.CrearTablaIr;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.EliminarTablaIr;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.UpdateParametroLaboral;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetParametrosLaborales;
using MipymeAsistencia.Application.Features.ConfiguracionLaboral.Queries.GetTablaIr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionLaboralController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConfiguracionLaboralController(IMediator mediator) => _mediator = mediator;

    // ── PARÁMETROS LABORALES ───────────────────────────────────────────────────

    /// <summary>
    /// Obtiene todos los parámetros laborales configurados (INSS, INATEC, horas mes, provisión prestaciones).
    /// </summary>
    [HttpGet("parametros")]
    [ProducesResponseType(typeof(ApiResponse<List<ParametroLaboralDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParametros()
    {
        var data = await _mediator.Send(new GetParametrosLaboralesQuery());
        return Ok(ApiResponse<List<ParametroLaboralDto>>.Ok(data, "Parámetros laborales obtenidos correctamente."));
    }

    /// <summary>
    /// Actualiza el valor de un parámetro laboral específico por su clave (ej. INSS_LABORAL, INSS_PATRONAL, INATEC, HORAS_LABORALES_MES).
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPut("parametros/{clave}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ParametroLaboralDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParametro(string clave, [FromBody] UpdateParametroLaboralRequestDto request)
    {
        var data = await _mediator.Send(new UpdateParametroLaboralCommand
        {
            Clave       = clave,
            Valor       = request.Valor,
            Descripcion = request.Descripcion
        });

        return Ok(ApiResponse<ParametroLaboralDto>.Ok(data, $"Parámetro '{clave}' actualizado correctamente."));
    }

    // ── TABLA DE IMPUESTO SOBRE LA RENTA (IR) ──────────────────────────────────

    /// <summary>
    /// Obtiene los tramos progresivos de la tabla de IR.
    /// </summary>
    [HttpGet("tabla-ir")]
    [ProducesResponseType(typeof(ApiResponse<List<TablaIrDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTablaIr([FromQuery] int? anio, [FromQuery] bool soloActivos = true)
    {
        var data = await _mediator.Send(new GetTablaIrQuery { Anio = anio, SoloActivos = soloActivos });
        return Ok(ApiResponse<List<TablaIrDto>>.Ok(data, $"Se encontraron {data.Count} tramos de IR."));
    }

    /// <summary>
    /// Agrega un nuevo tramo a la tabla de retención de IR.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost("tabla-ir")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TablaIrDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearTramoIr([FromBody] CrearTablaIrRequestDto request)
    {
        var data = await _mediator.Send(new CrearTablaIrCommand
        {
            DesdeMontoAnual     = request.DesdeMontoAnual,
            HastaMontoAnual     = request.HastaMontoAnual,
            PorcentajeAplicable = request.PorcentajeAplicable,
            MontoBaseExceso     = request.MontoBaseExceso,
            CuotaFija           = request.CuotaFija,
            AnioVigencia        = request.AnioVigencia,
            Activo              = request.Activo
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<TablaIrDto>.Created(data, "Tramo de IR registrado correctamente."));
    }

    /// <summary>
    /// Actualiza un tramo de la tabla de IR existente.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPut("tabla-ir/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TablaIrDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarTramoIr(int id, [FromBody] ActualizarTablaIrRequestDto request)
    {
        var data = await _mediator.Send(new ActualizarTablaIrCommand
        {
            IdTablaIr           = id,
            DesdeMontoAnual     = request.DesdeMontoAnual,
            HastaMontoAnual     = request.HastaMontoAnual,
            PorcentajeAplicable = request.PorcentajeAplicable,
            MontoBaseExceso     = request.MontoBaseExceso,
            CuotaFija           = request.CuotaFija,
            AnioVigencia        = request.AnioVigencia,
            Activo              = request.Activo
        });

        return Ok(ApiResponse<TablaIrDto>.Ok(data, "Tramo de IR actualizado correctamente."));
    }

    /// <summary>
    /// Elimina un tramo de la tabla de IR.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpDelete("tabla-ir/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarTramoIr(int id)
    {
        var result = await _mediator.Send(new EliminarTablaIrCommand { IdTablaIr = id });
        return Ok(ApiResponse<bool>.Ok(result, "Tramo de IR eliminado correctamente."));
    }
}
