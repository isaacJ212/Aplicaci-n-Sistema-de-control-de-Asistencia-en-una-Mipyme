using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;
using MipymeAsistencia.Application.Features.Planilla.Queries.GetPlanillasByEmpleado;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanillaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlanillaController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Genera la planilla mensual de un empleado aplicando:
    /// INSS 7%, IR tabla progresiva (Ley 822 LCT), horas extras aprobadas
    /// del periodo, aportes patronales y prestaciones sociales.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PlanillaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Generar([FromBody] GenerarPlanillaRequestDto request)
    {
        var data = await _mediator.Send(new GenerarPlanillaCommand
        {
            IdEmpleado       = request.IdEmpleado,
            PeriodoMesAnio   = request.PeriodoMesAnio,
            Comisiones       = request.Comisiones,
            Incentivos       = request.Incentivos,
            Embargo          = request.Embargo,
            Sindicato        = request.Sindicato,
            OtrasDeducciones = request.OtrasDeducciones
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<PlanillaResponseDto>.Created(
                data, $"Planilla del periodo {request.PeriodoMesAnio} generada correctamente."));
    }

    /// <summary>
    /// Obtiene el historial de planillas de un empleado.
    /// Opcionalmente filtra por periodo YYYY-MM.
    /// </summary>
    [HttpGet("empleado/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<PlanillaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmpleado(
        int idEmpleado,
        [FromQuery] string? periodo = null)
    {
        var data = await _mediator.Send(new GetPlanillasByEmpleadoQuery
        {
            IdEmpleado     = idEmpleado,
            PeriodoMesAnio = periodo
        });

        return Ok(ApiResponse<List<PlanillaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} planillas."));
    }
}
