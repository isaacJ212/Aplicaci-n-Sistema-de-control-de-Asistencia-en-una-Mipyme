using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluacionById;

public class GetEvaluacionByIdQueryHandler
    : IRequestHandler<GetEvaluacionByIdQuery, EvaluacionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetEvaluacionByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<EvaluacionResponseDto> Handle(
        GetEvaluacionByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _context.EvaluacionesDesempeno
            .Include(x => x.Empleado)
            .Include(x => x.Evaluador)
            .Include(x => x.Respuestas)
            .FirstOrDefaultAsync(x => x.IdEvaluacion == request.IdEvaluacion, cancellationToken)
            ?? throw new KeyNotFoundException("La evaluación no existe.");

        return new EvaluacionResponseDto
        {
            IdEvaluacion    = e.IdEvaluacion,
            IdEmpleado      = e.IdEmpleado,
            NombreEmpleado  = e.Empleado is null ? string.Empty
                              : $"{e.Empleado.Nombres} {e.Empleado.Apellidos}".Trim(),
            IdEvaluador     = e.IdEvaluador,
            NombreEvaluador = e.Evaluador?.Email ?? string.Empty,
            Perspectiva     = e.Perspectiva,
            Periodo         = e.Periodo,
            PuntajeFinal    = e.PuntajeFinal,
            Observaciones   = e.Observaciones,
            Estado          = e.Estado,
            FechaCreacion   = e.FechaCreacion,
            FechaCompletada = e.FechaCompletada,
            Respuestas      = e.Respuestas
                .OrderBy(r => r.NumeroPregunta)
                .Select(r => new RespuestaDto
                {
                    NumeroPregunta = r.NumeroPregunta,
                    Calificacion   = r.Calificacion,
                }).ToList(),
        };
    }
}
