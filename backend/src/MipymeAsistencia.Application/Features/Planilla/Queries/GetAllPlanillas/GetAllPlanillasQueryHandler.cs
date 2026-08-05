using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Planilla.Queries.GetAllPlanillas;

public class GetAllPlanillasQueryHandler
    : IRequestHandler<GetAllPlanillasQuery, List<PlanillaResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPlanillasQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<PlanillaResponseDto>> Handle(
        GetAllPlanillasQuery request, CancellationToken cancellationToken)
    {
        // ── 1. Cargar planillas desde BD SOLO con columnas existentes ───────
        var query = _context.HistorialPlanillas
            .Include(p => p.Empleado)
            .AsQueryable();

        if (request.IdEmpleado.HasValue)
            query = query.Where(p => p.IdEmpleado == request.IdEmpleado.Value);

        if (!string.IsNullOrWhiteSpace(request.PeriodoMesAnio))
            query = query.Where(p => p.PeriodoMesAnio == request.PeriodoMesAnio);

        var planillas = await query
            .OrderByDescending(p => p.PeriodoMesAnio)
            .ThenBy(p => p.Empleado!.Apellidos)
            .ToListAsync(cancellationToken);

        if (!planillas.Any()) return new List<PlanillaResponseDto>();

        // ── 2. Agrupar claves (IdEmpleado + Periodo) para calcular tardanzas ─
        var claves = planillas
            .Select(p => new { p.IdEmpleado, p.PeriodoMesAnio, p.SalarioBase })
            .Distinct()
            .ToList();

        // ── 3. Calcular MinutosTardanza y DeduccionTardanza por cada clave ───
        //    Valor por minuto = SalarioBase / 240h / 60min
        var tardanzaPorClave = new Dictionary<(int IdEmpleado, string Periodo), (int Minutos, decimal Monto)>();

        foreach (var clave in claves)
        {
            var partes = clave.PeriodoMesAnio.Split('-');
            if (partes.Length != 2) continue;

            if (!int.TryParse(partes[0], out var anio) || !int.TryParse(partes[1], out var mes))
                continue;

            var inicioPeriodo = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
            var finPeriodo    = inicioPeriodo.AddMonths(1).AddTicks(-1);

            var minutos = await _context.HistorialAsistencias
                .Where(h => h.IdEmpleado     == clave.IdEmpleado &&
                            h.Fecha          >= inicioPeriodo    &&
                            h.Fecha          <= finPeriodo        &&
                            h.MinutosTardanza > 0)
                .SumAsync(h => (int?)h.MinutosTardanza ?? 0, cancellationToken);

            var valorPorMinuto = clave.SalarioBase > 0 ? clave.SalarioBase / 240m / 60m : 0m;
            var monto          = Math.Round(valorPorMinuto * minutos, 2);

            tardanzaPorClave[(clave.IdEmpleado, clave.PeriodoMesAnio)] = (minutos, monto);
        }

        // ── 4. Construir DTOs en memoria ─────────────────────────────────────
        return planillas.Select(p =>
        {
            var tieneTardanza = tardanzaPorClave.TryGetValue(
                (p.IdEmpleado, p.PeriodoMesAnio), out var td);

            var minutosTardanza = tieneTardanza ? td.Minutos : 0;
            var deduccTardanza  = tieneTardanza ? td.Monto   : 0m;
            var prestacion      = Math.Round(p.SalarioBase / 30m * 2.5m, 2);

            return new PlanillaResponseDto
            {
                IdPlanilla             = p.IdPlanilla,
                IdEmpleado             = p.IdEmpleado,
                NombreEmpleado         = p.Empleado!.Nombres + " " + p.Empleado.Apellidos,
                CargoEmpleado          = p.Empleado.CargoFuncion,
                PeriodoMesAnio         = p.PeriodoMesAnio,
                SalarioBase            = p.SalarioBase,
                Comisiones             = 0m,
                TotalHorasExtras       = p.TotalHorasExtras,
                PagoHorasExtras        = p.PagoHorasExtras,
                Incentivos             = 0m,
                TotalIngresos          = p.SalarioBruto,
                InssLaboral            = p.InssLaboral,
                IrLaboral              = p.IrLaboral,
                DeduccionTardanza      = deduccTardanza,    // ← calculado desde Asistencias
                MinutosTardanzaMes     = minutosTardanza,    // ← calculado desde Asistencias
                Embargo                = 0m,                  // (no hay columna BD)
                Sindicato              = 0m,                  // (no hay columna BD)
                OtrasDeducciones       = p.OtrasDeducciones,
                TotalDeducciones       = p.TotalDeducciones,
                SalarioNeto            = p.SalarioNeto,
                InssPatronal           = Math.Round(p.SalarioBruto * 0.215m, 2),
                Inatec                 = Math.Round(p.SalarioBruto * 0.02m,  2),
                AcumuladoVacaciones    = prestacion,
                AcumuladoAguinaldo     = p.AcumuladoAguinaldo,
                AcumuladoIndemnizacion = prestacion,
                FechaEmision           = p.FechaEmision
            };
        }).ToList();
    }
}
