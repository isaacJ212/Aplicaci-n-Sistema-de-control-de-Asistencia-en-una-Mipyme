using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Empleado;
using MipymeAsistencia.Application.Common.Interfaces;
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
            NumeroInss = request.NumeroInss,
            EstadoCivil = request.EstadoCivil,
            EstadoEmpleado = request.EstadoEmpleado,
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
            NumeroInss = request.NumeroInss,
            EstadoCivil = request.EstadoCivil,
            EstadoEmpleado = request.EstadoEmpleado,
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

    /// <summary>
    /// Sube la foto de perfil de un empleado a Supabase Storage.
    /// El archivo se guarda con el nombre: {nombres}_{apellidos}.{ext}
    /// Retorna la URL pública del objeto y la actualiza en la BD.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost("{idEmpleado:int}/foto")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<FotoEmpleadoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadFoto(
        int idEmpleado,
        IFormFile foto,
        [FromServices] IStorageService storage,
        [FromServices] IApplicationDbContext context)
    {
        if (foto is null || foto.Length == 0)
            return BadRequest(ApiResponse<object>.BadRequest("No se recibió ningún archivo."));

        // Validar tipo MIME
        var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!tiposPermitidos.Contains(foto.ContentType.ToLower()))
            return BadRequest(ApiResponse<object>.BadRequest(
                "Tipo de archivo no permitido. Usa JPG, PNG, WEBP o GIF."));

        // Validar tamaño (máx 5 MB)
        if (foto.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.BadRequest(
                "El archivo supera el tamaño máximo permitido de 5 MB."));

        // Buscar el empleado
        var empleado = await context.Empleados
            .FindAsync(new object[] { idEmpleado });

        if (empleado is null)
            return NotFound(ApiResponse<object>.NotFound(
                $"Empleado con id {idEmpleado} no encontrado."));

        // Construir nombre del archivo: nombres_apellidos.ext
        // Ej: "Carlos Ramirez" -> "carlos_ramirez.jpg"
        var ext         = Path.GetExtension(foto.FileName).TrimStart('.');
        var nombreBase  = $"{empleado.Nombres}_{empleado.Apellidos}"
                            .ToLower()
                            .Replace(' ', '_')
                            .Replace('á','a').Replace('é','e').Replace('í','i')
                            .Replace('ó','o').Replace('ú','u').Replace('ñ','n');
        var fileName    = $"{nombreBase}.{ext}";

        // Leer bytes del archivo
        using var ms = new MemoryStream();
        await foto.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        // Subir a Supabase Storage
        var fotoUrl = await storage.UploadAsync(fileName, fileBytes, foto.ContentType);

        // Actualizar la URL en la BD
        empleado.FotoUrl = fotoUrl;
        await context.SaveChangesAsync(CancellationToken.None);

        return Ok(ApiResponse<FotoEmpleadoResponseDto>.Ok(
            new FotoEmpleadoResponseDto
            {
                IdEmpleado     = empleado.IdEmpleado,
                NombreArchivo  = fileName,
                FotoUrl        = fotoUrl
            },
            "Foto subida y guardada correctamente."));
    }
}
