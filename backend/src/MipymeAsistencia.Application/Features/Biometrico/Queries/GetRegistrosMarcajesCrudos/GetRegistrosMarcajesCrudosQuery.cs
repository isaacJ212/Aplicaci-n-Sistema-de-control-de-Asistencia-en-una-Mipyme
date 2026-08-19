using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Queries.GetRegistrosMarcajesCrudos;

public class GetRegistrosMarcajesCrudosQuery : IRequest<List<RegistroMarcajeBiometricoDto>>
{
    public int? IdDispositivo { get; set; }
    public int Limite { get; set; } = 50;
}

public class GetRegistrosMarcajesCrudosQueryHandler : IRequestHandler<GetRegistrosMarcajesCrudosQuery, List<RegistroMarcajeBiometricoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRegistrosMarcajesCrudosQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<RegistroMarcajeBiometricoDto>> Handle(GetRegistrosMarcajesCrudosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.RegistrosMarcajesBiometricos
            .Include(r => r.Dispositivo)
            .AsNoTracking();

        if (request.IdDispositivo.HasValue)
        {
            query = query.Where(r => r.IdDispositivo == request.IdDispositivo.Value);
        }

        var list = await query
            .OrderByDescending(r => r.FechaHora)
            .Take(Math.Min(200, Math.Max(1, request.Limite)))
            .ToListAsync(cancellationToken);

        var empleados = await _context.Empleados
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return list.Select(r =>
        {
            var emp = empleados.FirstOrDefault(e =>
                e.CedulaIdentificacion.Equals(r.NumeroEnrollamiento, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(e.NumeroInss) && e.NumeroInss.Equals(r.NumeroEnrollamiento, StringComparison.OrdinalIgnoreCase)) ||
                e.IdEmpleado.ToString() == r.NumeroEnrollamiento);

            return new RegistroMarcajeBiometricoDto
            {
                IdRegistroBiometrico = r.IdRegistroBiometrico,
                IdDispositivo        = r.IdDispositivo,
                NombreDispositivo    = r.Dispositivo?.NombreDispositivo ?? $"Dispositivo #{r.IdDispositivo}",
                NumeroEnrollamiento  = r.NumeroEnrollamiento,
                NombreEmpleado       = emp != null ? $"{emp.Nombres} {emp.Apellidos}" : "No asignado",
                FechaHora            = r.FechaHora,
                TipoMarcaje          = r.TipoMarcaje,
                TipoVerificacion     = r.TipoVerificacion,
                Procesado            = r.Procesado,
                FechaProcesado       = r.FechaProcesado,
                IdAsistenciaGenerada = r.IdAsistenciaGenerada,
                ErrorProcesamiento   = r.ErrorProcesamiento
            };
        }).ToList();
    }
}
