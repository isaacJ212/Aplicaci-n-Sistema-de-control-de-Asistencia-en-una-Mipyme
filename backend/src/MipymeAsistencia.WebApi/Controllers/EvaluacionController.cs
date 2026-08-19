using System.Security.Claims;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.Features.Evaluacion.Commands.CrearEvaluacion;
using MipymeAsistencia.Application.Features.Evaluacion.Commands.ResponderEvaluacion;
using MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluacionById;
using MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluaciones;
using MipymeAsistencia.Application.Features.Evaluacion.Queries.GetPreguntas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EvaluacionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public EvaluacionController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context  = context;
    }


    private async Task<int?> ObtenerIdUsuarioDelJwt(CancellationToken ct = default)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(email)) return null;
        var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == email, ct);
        return u?.IdUsuario;
    }


    [HttpGet("preguntas")]
    [ProducesResponseType(typeof(ApiResponse<List<PreguntaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreguntas()
    {
        var data = await _mediator.Send(new GetPreguntasQuery());
        return Ok(ApiResponse<List<PreguntaDto>>.Ok(data, $"{data.Count} preguntas del formulario 360°."));
    }

 
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EvaluacionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int?    idEmpleado  = null,
        [FromQuery] int?    idEvaluador = null,
        [FromQuery] string? periodo     = null,
        [FromQuery] string? estado      = null,
        CancellationToken ct = default)
    {
        // Si el rol es Empleado, forzamos que solo vea sus propias evaluaciones
        if (User.IsInRole("Empleado"))
        {
            var idUsuario = await ObtenerIdUsuarioDelJwt(ct);
            idEvaluador = idUsuario;  // evaluaciones donde es el evaluador (sus propias)
        }

        var data = await _mediator.Send(new GetEvaluacionesQuery
        {
            IdEmpleado  = idEmpleado,
            IdEvaluador = idEvaluador,
            Periodo     = periodo,
            Estado      = estado,
        });

        return Ok(ApiResponse<List<EvaluacionResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} evaluaciones."));
    }

 
    [HttpGet("{idEvaluacion:int}")]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int idEvaluacion)
    {
        var data = await _mediator.Send(new GetEvaluacionByIdQuery { IdEvaluacion = idEvaluacion });
        return Ok(ApiResponse<EvaluacionResponseDto>.Ok(data, "Evaluación obtenida correctamente."));
    }


    [HttpPost]
    [Authorize(Roles = "Admin,Analista")]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearEvaluacionRequestDto request,
        CancellationToken ct = default)
    {
        if (request.IdEmpleado <= 0)
            return BadRequest(ApiResponse<object>.BadRequest("El empleado es obligatorio."));

        if (string.IsNullOrWhiteSpace(request.Periodo))
            return BadRequest(ApiResponse<object>.BadRequest("El período es obligatorio. Ej: 2026-S1"));


        var idEvaluador = request.IdEvaluadorOverride ?? 0;
        if (idEvaluador <= 0)
        {
            var idJwt = await ObtenerIdUsuarioDelJwt(ct);
            if (!idJwt.HasValue)
                return Unauthorized(ApiResponse<object>.Unauthorized("No se pudo identificar al evaluador."));
            idEvaluador = idJwt.Value;
        }

        var data = await _mediator.Send(new CrearEvaluacionCommand
        {
            IdEmpleado  = request.IdEmpleado,
            IdEvaluador = idEvaluador,
            Perspectiva = request.Perspectiva,
            Periodo     = request.Periodo,
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EvaluacionResponseDto>.Created(data, "Formulario de evaluación creado correctamente."));
    }


    [HttpPut("{idEvaluacion:int}/responder")]
    [ProducesResponseType(typeof(ApiResponse<EvaluacionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Responder(
        int idEvaluacion,
        [FromBody] ResponderEvaluacionRequestDto request,
        CancellationToken ct = default)
    {
        // Verificar que quien responde es el evaluador asignado (a menos que sea Admin/Analista)
        if (!User.IsInRole("Admin") && !User.IsInRole("Analista"))
        {
            var idUsuario = await ObtenerIdUsuarioDelJwt(ct);
            var eval = await _context.EvaluacionesDesempeno
                .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion, ct);

            if (eval is null)
                return NotFound(ApiResponse<object>.NotFound("La evaluación no existe."));

            if (eval.IdEvaluador != idUsuario)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Unauthorized("Solo el evaluador asignado puede responder este formulario."));
        }

        var data = await _mediator.Send(new ResponderEvaluacionCommand
        {
            IdEvaluacion  = idEvaluacion,
            Respuestas    = request.Respuestas,
            Observaciones = request.Observaciones,
        });

        return Ok(ApiResponse<EvaluacionResponseDto>.Ok(
            data, $"Evaluación completada. Puntaje final: {data.PuntajeFinal}%"));
    }
}
