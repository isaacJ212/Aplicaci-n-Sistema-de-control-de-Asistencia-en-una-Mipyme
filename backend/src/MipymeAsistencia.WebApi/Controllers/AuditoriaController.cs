using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Analista")]
public class AuditoriaController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AuditoriaController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el historial de auditoría filtrado por entidad e ID de registro.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AuditoriaLog>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? entidad = null,
        [FromQuery] int? idRegistro = null,
        [FromQuery] int limite = 50,
        CancellationToken ct = default)
    {
        var query = _context.AuditoriaLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(x => x.Entidad.ToLower() == entidad.ToLower());

        if (idRegistro.HasValue && idRegistro.Value > 0)
            query = query.Where(x => x.IdRegistro == idRegistro.Value);

        var logs = await query
            .OrderByDescending(x => x.FechaEvento)
            .Take(limite)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<AuditoriaLog>>.Ok(logs, $"Se obtuvieron {logs.Count} registros de auditoría."));
    }

    /// <summary>
    /// Obtiene el historial de auditoría de un empleado específico.
    /// </summary>
    [HttpGet("empleado/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditoriaLog>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogsEmpleado(int idEmpleado, CancellationToken ct = default)
    {
        var logs = await _context.AuditoriaLogs
            .AsNoTracking()
            .Where(x => x.Entidad == "Empleado" && x.IdRegistro == idEmpleado)
            .OrderByDescending(x => x.FechaEvento)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<AuditoriaLog>>.Ok(logs, $"Historial de auditoría para el empleado #{idEmpleado}."));
    }
}
