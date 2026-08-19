using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using MipymeAsistencia.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Evaluacion.Commands.ResponderEvaluacion;

public class ResponderEvaluacionCommandHandler
    : IRequestHandler<ResponderEvaluacionCommand, EvaluacionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public ResponderEvaluacionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<EvaluacionResponseDto> Handle(
        ResponderEvaluacionCommand request, CancellationToken cancellationToken)
    {
        var evaluacion = await _context.EvaluacionesDesempeno
            .Include(e => e.Empleado)
            .Include(e => e.Evaluador)
            .Include(e => e.Respuestas)
            .FirstOrDefaultAsync(e => e.IdEvaluacion == request.IdEvaluacion, cancellationToken)
            ?? throw new KeyNotFoundException("La evaluación no existe.");

        if (evaluacion.Estado == "Completada")
            throw new InvalidOperationException("Esta evaluación ya fue respondida.");

        if (request.Respuestas.Count != 20)
            throw new ArgumentException("Se requieren exactamente 20 respuestas (una por pregunta).");

        // Validar numeración y calificaciones
        var numeros = request.Respuestas.Select(r => r.NumeroPregunta).ToHashSet();
        if (numeros.Count != 20 || numeros.Min() < 1 || numeros.Max() > 20)
            throw new ArgumentException("Las respuestas deben cubrir las preguntas 1 a 20 sin repeticiones.");

        if (request.Respuestas.Any(r => r.Calificacion < 1 || r.Calificacion > 5))
            throw new ArgumentException("Cada calificación debe estar entre 1 y 5.");

        // Limpiar respuestas anteriores (en caso de re-envío parcial)
        _context.EvaluacionRespuestas.RemoveRange(evaluacion.Respuestas);

        // Persistir nuevas respuestas
        var respuestasEntidad = request.Respuestas.Select(r => new EvaluacionRespuesta
        {
            IdEvaluacion   = evaluacion.IdEvaluacion,
            NumeroPregunta = r.NumeroPregunta,
            Calificacion   = r.Calificacion,
        }).ToList();

        _context.EvaluacionRespuestas.AddRange(respuestasEntidad);

        // Calcular puntaje con la fórmula ponderada
        var calificaciones = request.Respuestas
            .ToDictionary(r => r.NumeroPregunta, r => r.Calificacion);

        evaluacion.PuntajeFinal    = Evaluacion360Preguntas.CalcularPuntaje(calificaciones);
        evaluacion.Observaciones   = request.Observaciones?.Trim();
        evaluacion.Estado          = "Completada";
        evaluacion.FechaCompletada = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new EvaluacionResponseDto
        {
            IdEvaluacion    = evaluacion.IdEvaluacion,
            IdEmpleado      = evaluacion.IdEmpleado,
            NombreEmpleado  = evaluacion.Empleado is null ? string.Empty
                              : $"{evaluacion.Empleado.Nombres} {evaluacion.Empleado.Apellidos}".Trim(),
            IdEvaluador     = evaluacion.IdEvaluador,
            NombreEvaluador = evaluacion.Evaluador?.Email ?? string.Empty,
            Perspectiva     = evaluacion.Perspectiva,
            Periodo         = evaluacion.Periodo,
            PuntajeFinal    = evaluacion.PuntajeFinal,
            Observaciones   = evaluacion.Observaciones,
            Estado          = evaluacion.Estado,
            FechaCreacion   = evaluacion.FechaCreacion,
            FechaCompletada = evaluacion.FechaCompletada,
            Respuestas      = respuestasEntidad.Select(r => new RespuestaDto
            {
                NumeroPregunta = r.NumeroPregunta,
                Calificacion   = r.Calificacion,
            }).OrderBy(r => r.NumeroPregunta).ToList(),
        };
    }
}
