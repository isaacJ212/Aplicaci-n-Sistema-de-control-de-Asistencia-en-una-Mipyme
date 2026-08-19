using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Evaluacion.Commands.CrearEvaluacion;

public class CrearEvaluacionCommandHandler
    : IRequestHandler<CrearEvaluacionCommand, EvaluacionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public CrearEvaluacionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<EvaluacionResponseDto> Handle(
        CrearEvaluacionCommand request, CancellationToken cancellationToken)
    {
        var perspectivasValidas = new[] { "Autoevaluacion", "Jefe", "Par", "Subordinado" };
        if (!perspectivasValidas.Contains(request.Perspectiva))
            throw new ArgumentException(
                "Perspectiva inválida. Debe ser: Autoevaluacion, Jefe, Par o Subordinado.");

        if (string.IsNullOrWhiteSpace(request.Periodo))
            throw new ArgumentException("El período es obligatorio. Ej: 2026-S1");

        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken)
            ?? throw new KeyNotFoundException("El empleado no existe.");

        var evaluador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == request.IdEvaluador, cancellationToken)
            ?? throw new KeyNotFoundException("El evaluador no existe.");

        // Evitar duplicado del mismo evaluador para el mismo empleado/perspectiva/período
        var existe = await _context.EvaluacionesDesempeno
            .AnyAsync(e => e.IdEmpleado  == request.IdEmpleado
                        && e.IdEvaluador  == request.IdEvaluador
                        && e.Perspectiva  == request.Perspectiva
                        && e.Periodo      == request.Periodo, cancellationToken);

        if (existe)
            throw new InvalidOperationException(
                $"Ya existe una evaluación de {request.Perspectiva} para este empleado en {request.Periodo}.");

        var evaluacion = new EvaluacionDesempeno
        {
            IdEmpleado   = request.IdEmpleado,
            IdEvaluador  = request.IdEvaluador,
            Perspectiva  = request.Perspectiva,
            Periodo      = request.Periodo.Trim(),
            Estado       = "Pendiente",
            FechaCreacion = DateTime.UtcNow,
        };

        _context.EvaluacionesDesempeno.Add(evaluacion);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(evaluacion, empleado, evaluador);
    }

    private static EvaluacionResponseDto MapToDto(
        EvaluacionDesempeno e, Domain.Entities.Empleado emp, Domain.Entities.Usuario eval) => new()
    {
        IdEvaluacion    = e.IdEvaluacion,
        IdEmpleado      = e.IdEmpleado,
        NombreEmpleado  = $"{emp.Nombres} {emp.Apellidos}".Trim(),
        IdEvaluador     = e.IdEvaluador,
        NombreEvaluador = eval.Email,
        Perspectiva     = e.Perspectiva,
        Periodo         = e.Periodo,
        PuntajeFinal    = e.PuntajeFinal,
        Observaciones   = e.Observaciones,
        Estado          = e.Estado,
        FechaCreacion   = e.FechaCreacion,
        FechaCompletada = e.FechaCompletada,
    };
}
