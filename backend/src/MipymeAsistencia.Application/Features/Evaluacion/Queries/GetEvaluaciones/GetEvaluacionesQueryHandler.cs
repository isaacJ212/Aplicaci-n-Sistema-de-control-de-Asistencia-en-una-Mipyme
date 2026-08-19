using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetEvaluaciones;

public class GetEvaluacionesQueryHandler
    : IRequestHandler<GetEvaluacionesQuery, List<EvaluacionResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEvaluacionesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<EvaluacionResponseDto>> Handle(
        GetEvaluacionesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EvaluacionesDesempeno
            .Include(e => e.Empleado)
            .Include(e => e.Evaluador)
            .AsQueryable();

        if (request.IdEmpleado.HasValue)
            query = query.Where(e => e.IdEmpleado == request.IdEmpleado.Value);

        if (request.IdEvaluador.HasValue)
            query = query.Where(e => e.IdEvaluador == request.IdEvaluador.Value);

        if (!string.IsNullOrWhiteSpace(request.Periodo))
            query = query.Where(e => e.Periodo == request.Periodo);

        if (!string.IsNullOrWhiteSpace(request.Estado))
            query = query.Where(e => e.Estado == request.Estado);

        var lista = await query
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(cancellationToken);

        return lista.Select(e => new EvaluacionResponseDto
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
        }).ToList();
    }
}
