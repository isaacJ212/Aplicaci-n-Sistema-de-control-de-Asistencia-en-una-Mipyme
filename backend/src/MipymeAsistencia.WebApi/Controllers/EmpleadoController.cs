using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Features.Empleado.Commands.CreateEmpleado;
using MipymeAsistencia.Application.Features.Empleado.Commands.DeleteEmpleado;
using MipymeAsistencia.Application.Features.Empleado.Commands.UpdateEmpleado;
using MipymeAsistencia.Application.Features.Empleado.Queries.GetAllEmpleados;
using MipymeAsistencia.Application.Features.Empleado.Queries.GetEmpleadoById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadoController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmpleadoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmpleadoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var data = await _mediator.Send(new GetAllEmpleadosQuery());
        return Ok(ApiResponse<List<EmpleadoResponseDto>>.Ok(data, "Lista de empleados obtenida correctamente."));
    }

    [HttpGet("{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmpleadoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int idEmpleado)
    {
        var data = await _mediator.Send(new GetEmpleadoByIdQuery { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<EmpleadoResponseDto>.Ok(data, "Empleado obtenido correctamente."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EmpleadoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateEmpleadoRequestDto request)
    {
        var data = await _mediator.Send(new CreateEmpleadoCommand
        {
            IdUsuario = request.IdUsuario,
            CedulaIdentificacion = request.CedulaIdentificacion,
            FotoUrl = request.FotoUrl,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            CargoFuncion = request.CargoFuncion,
            Responsabilidades = request.Responsabilidades,
            FechaContratacion = request.FechaContratacion,
            SalarioBaseMensual = request.SalarioBaseMensual,
            DiasVacacionesAcumuladas = request.DiasVacacionesAcumuladas
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EmpleadoResponseDto>.Created(data, "Empleado creado correctamente."));
    }

    [HttpPut("{idEmpleado:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<EmpleadoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int idEmpleado, [FromBody] UpdateEmpleadoRequestDto request)
    {
        var data = await _mediator.Send(new UpdateEmpleadoCommand
        {
            IdEmpleado = idEmpleado,
            CedulaIdentificacion = request.CedulaIdentificacion,
            FotoUrl = request.FotoUrl,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            CargoFuncion = request.CargoFuncion,
            Responsabilidades = request.Responsabilidades,
            FechaContratacion = request.FechaContratacion,
            SalarioBaseMensual = request.SalarioBaseMensual,
            DiasVacacionesAcumuladas = request.DiasVacacionesAcumuladas
        });

        return Ok(ApiResponse<EmpleadoResponseDto>.Ok(data, "Empleado actualizado correctamente."));
    }

    [HttpDelete("{idEmpleado:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idEmpleado)
    {
        await _mediator.Send(new DeleteEmpleadoCommand { IdEmpleado = idEmpleado });
        return Ok(ApiResponse<object>.Ok(null!, "Empleado eliminado correctamente."));
    }
}
