using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetAlertasTardanza;

public class GetAlertasTardanzaQueryHandler
    : IRequestHandler<GetAlertasTardanzaQuery, List<AlertaTardanzaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertasTardanzaQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<AlertaTardanzaDto>> Handle(
        GetAlertasTardanzaQuery request, CancellationToken cancellationToken)
    {
        // Default: mes actual
        var periodo = string.IsNullOrWhiteSpace(request.PeriodoMesAnio)
            ? $"{DateTime.UtcNow:yyyy-MM}"
            : request.PeriodoMesAnio;

        var partes = periodo.Split('-');
        var anio   = int.Parse(partes[0]);
        var mes    = int.Parse(partes[1]);

        var inicioPeriodo = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var finPeriodo    = inicioPeriodo.AddMonths(1).AddTicks(-1);

        // Traer todos los registros de asistencia del período con tardanza > 0
        var tardanzas = await _context.HistorialAsistencias
            .Include(a => a.Empleado)
            .Where(a =>
                a.Fecha >= inicioPeriodo &&
                a.Fecha <= finPeriodo    &&
                a.MinutosTardanza > 0)
            .ToListAsync(cancellationToken);

        if (!tardanzas.Any())
            return new List<AlertaTardanzaDto>();

        // Traer salarios de empleados para calcular la deducción
        var idEmpleados = tardanzas.Select(t => t.IdEmpleado).Distinct().ToList();
        var empleados   = await _context.Empleados
            .Where(e => idEmpleados.Contains(e.IdEmpleado))
            .ToListAsync(cancellationToken);

        var salarioMap = empleados.ToDictionary(e => e.IdEmpleado, e => e.SalarioBaseMensual);

        // Agrupar por empleado
        var resultado = tardanzas
            .GroupBy(a => a.IdEmpleado)
            .Select(g =>
            {
                var emp            = g.First().Empleado!;
                var totalTardanzas = g.Count();
                var totalMinutos   = g.Sum(a => a.MinutosTardanza);
                var salario        = salarioMap.GetValueOrDefault(g.Key, 0m);

                // Deducción: (salario / 240h / 60min) * minutos_tardanza_total
                // = valor por minuto × total minutos tardanza
                var valorMinuto       = salario > 0 ? salario / 240m / 60m : 0m;
                var deduccion         = Math.Round(valorMinuto * totalMinutos, 2);

                return new AlertaTardanzaDto
                {
                    IdEmpleado        = g.Key,
                    NombreEmpleado    = $"{emp.Nombres} {emp.Apellidos}",
                    CargoFuncion      = emp.CargoFuncion,
                    PeriodoMesAnio    = periodo,
                    TotalTardanzas    = totalTardanzas,
                    TotalMinutos      = totalMinutos,
                    DeduccionTardanza = deduccion,
                    EsReincidente     = totalTardanzas >= request.UmbralReincidencia,
                };
            })
            .OrderByDescending(a => a.TotalTardanzas)
            .ToList();

        return resultado;
    }
}
