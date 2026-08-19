using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetInformeAsistencia;

public class GetInformeAsistenciaQueryHandler
    : IRequestHandler<GetInformeAsistenciaQuery, List<InformeAsistenciaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInformeAsistenciaQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<InformeAsistenciaDto>> Handle(
        GetInformeAsistenciaQuery request, CancellationToken cancellationToken)
    {
        // Normalizar rango al inicio/fin del día en UTC
        var desde = DateTime.SpecifyKind(request.FechaDesde.Date, DateTimeKind.Utc);
        var hasta = DateTime.SpecifyKind(request.FechaHasta.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Días laborales (lun-vie) en el período
        int diasLaborales = ContarDiasLaborales(desde, request.FechaHasta.Date);

        // Empleados a incluir
        var empleadosQuery = _context.Empleados.Include(e => e.Usuario).AsQueryable();
        if (request.IdEmpleado.HasValue)
            empleadosQuery = empleadosQuery.Where(e => e.IdEmpleado == request.IdEmpleado.Value);

        var empleados = await empleadosQuery
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres)
            .ToListAsync(cancellationToken);

        if (empleados.Count == 0)
            return [];

        var idsEmpleados = empleados.Select(e => e.IdEmpleado).ToList();

        // Registros de asistencia del período para todos los empleados
        var registros = await _context.HistorialAsistencias
            .Where(h => idsEmpleados.Contains(h.IdEmpleado)
                     && h.Fecha >= desde
                     && h.Fecha <= hasta)
            .ToListAsync(cancellationToken);

        var resultado = new List<InformeAsistenciaDto>();

        foreach (var emp in empleados)
        {
            var regsEmp = registros.Where(r => r.IdEmpleado == emp.IdEmpleado).ToList();

            int diasTrabajados       = regsEmp.Count;
            int diasTardanza         = regsEmp.Count(r => r.EstadoAsistencia == "Tardanza");
            int diasATiempo          = regsEmp.Count(r => r.EstadoAsistencia == "A Tiempo");
            int totalMinTardanza     = regsEmp.Sum(r => r.MinutosTardanza);
            int diasAusente          = Math.Max(0, diasLaborales - diasTrabajados);

            double pctPuntualidad    = diasTrabajados > 0
                ? Math.Round((double)diasATiempo / diasTrabajados * 100, 1)
                : 0;
            double pctAsistencia     = diasLaborales > 0
                ? Math.Round((double)diasTrabajados / diasLaborales * 100, 1)
                : 0;
            double promMinTardanza   = diasTardanza > 0
                ? Math.Round((double)totalMinTardanza / diasTardanza, 1)
                : 0;

            resultado.Add(new InformeAsistenciaDto
            {
                IdEmpleado               = emp.IdEmpleado,
                NombreEmpleado           = $"{emp.Nombres} {emp.Apellidos}".Trim(),
                CargoFuncion             = emp.CargoFuncion,
                FotoUrl                  = emp.FotoUrl,
                FechaDesde               = desde,
                FechaHasta               = request.FechaHasta.Date,
                DiasLaborales            = diasLaborales,
                DiasTrabajados           = diasTrabajados,
                DiasAusente              = diasAusente,
                DiasTardanza             = diasTardanza,
                DiasATiempo              = diasATiempo,
                TotalMinutosTardanza     = totalMinTardanza,
                PromedioMinutosTardanza  = promMinTardanza,
                PorcentajePuntualidad    = pctPuntualidad,
                PorcentajeAsistencia     = pctAsistencia,
            });
        }

        return resultado;
    }

    /// <summary>Cuenta días de lunes a viernes (inclusive) entre dos fechas.</summary>
    private static int ContarDiasLaborales(DateTime desde, DateTime hasta)
    {
        int count = 0;
        for (var d = desde.Date; d <= hasta.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }
        return count;
    }
}
