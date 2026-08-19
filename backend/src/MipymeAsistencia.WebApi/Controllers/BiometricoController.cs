using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Features.Biometrico.Commands.ActualizarDispositivoBiometrico;
using MipymeAsistencia.Application.Features.Biometrico.Commands.CrearDispositivoBiometrico;
using MipymeAsistencia.Application.Features.Biometrico.Commands.EliminarDispositivoBiometrico;
using MipymeAsistencia.Application.Features.Biometrico.Commands.IngestarMarcajesBiometricos;
using MipymeAsistencia.Application.Features.Biometrico.Commands.ProbarConexionDispositivo;
using MipymeAsistencia.Application.Features.Biometrico.Commands.SincronizarDispositivoBiometrico;
using MipymeAsistencia.Application.Features.Biometrico.Queries.GetDispositivoBiometricoById;
using MipymeAsistencia.Application.Features.Biometrico.Queries.GetDispositivosBiometricos;
using MipymeAsistencia.Application.Features.Biometrico.Queries.GetRegistrosMarcajesCrudos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BiometricoController : ControllerBase
{
    private readonly IMediator _mediator;

    public BiometricoController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lista todos los relojes marcadores y terminales biométricas configuradas.
    /// </summary>
    [HttpGet("dispositivos")]
    [ProducesResponseType(typeof(ApiResponse<List<DispositivoBiometricoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDispositivos()
    {
        var data = await _mediator.Send(new GetDispositivosBiometricosQuery());
        return Ok(ApiResponse<List<DispositivoBiometricoDto>>.Ok(data, $"Se encontraron {data.Count} dispositivos biométricos."));
    }

    /// <summary>
    /// Obtiene el detalle de un dispositivo biométrico por ID.
    /// </summary>
    [HttpGet("dispositivos/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DispositivoBiometricoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDispositivoById(int id)
    {
        var data = await _mediator.Send(new GetDispositivoBiometricoByIdQuery { IdDispositivo = id });
        return Ok(ApiResponse<DispositivoBiometricoDto>.Ok(data, "Dispositivo biométrico obtenido correctamente."));
    }

    /// <summary>
    /// Registra un nuevo reloj marcador biométrico en el sistema.
    /// </summary>
    [HttpPost("dispositivos")]
    [ProducesResponseType(typeof(ApiResponse<DispositivoBiometricoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearDispositivo([FromBody] CrearDispositivoBiometricoRequestDto request)
    {
        var data = await _mediator.Send(new CrearDispositivoBiometricoCommand
        {
            NombreDispositivo = request.NombreDispositivo,
            DireccionIp       = request.DireccionIp,
            Puerto            = request.Puerto,
            TipoProtocolo     = request.TipoProtocolo,
            Ubicacion         = request.Ubicacion,
            ClaveComunicacion = request.ClaveComunicacion,
            Activo            = request.Activo
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<DispositivoBiometricoDto>.Created(data, "Dispositivo biométrico registrado exitosamente."));
    }

    /// <summary>
    /// Actualiza los parámetros de red o configuración de un dispositivo biométrico.
    /// </summary>
    [HttpPut("dispositivos/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DispositivoBiometricoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarDispositivo(int id, [FromBody] ActualizarDispositivoBiometricoRequestDto request)
    {
        var data = await _mediator.Send(new ActualizarDispositivoBiometricoCommand
        {
            IdDispositivo     = id,
            NombreDispositivo = request.NombreDispositivo,
            DireccionIp       = request.DireccionIp,
            Puerto            = request.Puerto,
            TipoProtocolo     = request.TipoProtocolo,
            Ubicacion         = request.Ubicacion,
            ClaveComunicacion = request.ClaveComunicacion,
            Activo            = request.Activo
        });

        return Ok(ApiResponse<DispositivoBiometricoDto>.Ok(data, "Dispositivo biométrico actualizado exitosamente."));
    }

    /// <summary>
    /// Elimina un dispositivo biométrico del sistema.
    /// </summary>
    [HttpDelete("dispositivos/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarDispositivo(int id)
    {
        var result = await _mediator.Send(new EliminarDispositivoBiometricoCommand { IdDispositivo = id });
        return Ok(ApiResponse<bool>.Ok(result, "Dispositivo biométrico eliminado exitosamente."));
    }

    /// <summary>
    /// Prueba la conectividad IP y puerto con el reloj marcador físico.
    /// </summary>
    [HttpPost("dispositivos/{id:int}/test-conexion")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestConexion(int id)
    {
        var conectado = await _mediator.Send(new ProbarConexionDispositivoCommand { IdDispositivo = id });
        var mensaje = conectado
            ? "Conexión exitosa con el reloj biométrico."
            : "No se pudo conectar con el reloj biométrico. Revisa la IP, el puerto y que el dispositivo esté encendido en la red.";
        return Ok(ApiResponse<bool>.Ok(conectado, mensaje));
    }

    /// <summary>
    /// Ejecuta la sincronización de marcaciones pendientes desde los relojes biométricos hacia HistorialAsistencia.
    /// </summary>
    [HttpPost("sincronizar")]
    [ProducesResponseType(typeof(ApiResponse<ResultadoSincronizacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sincronizar([FromQuery] int? idDispositivo)
    {
        var data = await _mediator.Send(new SincronizarDispositivoBiometricoCommand { IdDispositivo = idDispositivo });
        return Ok(ApiResponse<ResultadoSincronizacionDto>.Ok(data, data.Mensaje));
    }

    /// <summary>
    /// Endpoint de ingestión por lotes (push) para agentes o servicios que extraigan marcaciones de hardware local.
    /// </summary>
    [HttpPost("ingestar-lote")]
    [AllowAnonymous] // Permite que el agente local en la red del reloj envíe marcaciones
    [ProducesResponseType(typeof(ApiResponse<ResultadoSincronizacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IngestarLote([FromBody] IngestarMarcajesRequestDto request)
    {
        var data = await _mediator.Send(new IngestarMarcajesBiometricosCommand
        {
            IdDispositivo = request.IdDispositivo,
            Marcajes      = request.Marcajes
        });

        return Ok(ApiResponse<ResultadoSincronizacionDto>.Ok(data, data.Mensaje));
    }

    /// <summary>
    /// Consulta los registros crudos de auditoría de marcajes biométricos.
    /// </summary>
    [HttpGet("registros-crudos")]
    [ProducesResponseType(typeof(ApiResponse<List<RegistroMarcajeBiometricoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegistrosCrudos([FromQuery] int? idDispositivo, [FromQuery] int limite = 50)
    {
        var data = await _mediator.Send(new GetRegistrosMarcajesCrudosQuery
        {
            IdDispositivo = idDispositivo,
            Limite        = limite
        });

        return Ok(ApiResponse<List<RegistroMarcajeBiometricoDto>>.Ok(data, $"Se obtuvieron {data.Count} registros de auditoría biométrica."));
    }
}
