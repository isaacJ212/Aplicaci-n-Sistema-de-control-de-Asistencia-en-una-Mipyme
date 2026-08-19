using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanillaPorDepartamento;

public class GenerarPlanillaPorDepartamentoCommandHandler
    : IRequestHandler<GenerarPlanillaPorDepartamentoCommand, GenerarPlanillaPorDepartamentoResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GenerarPlanillaPorDepartamentoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<GenerarPlanillaPorDepartamentoResponseDto> Handle(
        GenerarPlanillaPorDepartamentoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PeriodoMesAnio) || !request.PeriodoMesAnio.Contains('-'))
            throw new ArgumentException("El período debe tener el formato YYYY-MM.");

        var partes = request.PeriodoMesAnio.Split('-');
        if (!int.TryParse(partes[0], out var anio) || !int.TryParse(partes[1], out var mes))
            throw new ArgumentException("El período no es válido.");

        // 1. Obtener empleados activos
        var queryEmpleados = _context.Empleados
            .Include(e => e.Usuario)
            .Where(e => e.EstadoEmpleado == "Activo" && (e.Usuario == null || e.Usuario.EstadoActivo))
            .AsQueryable();

        var esTodos = string.IsNullOrWhiteSpace(request.Departamento) ||
                      request.Departamento.Equals("Todos", StringComparison.OrdinalIgnoreCase);

        if (!esTodos)
        {
            queryEmpleados = queryEmpleados.Where(e => e.Departamento == request.Departamento);
        }

        var empleados = await queryEmpleados
            .OrderBy(e => e.Departamento)
            .ThenBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync(cancellationToken);

        if (!empleados.Any())
        {
            return new GenerarPlanillaPorDepartamentoResponseDto
            {
                PeriodoMesAnio = request.PeriodoMesAnio,
                Departamento = esTodos ? "Todos" : request.Departamento!,
                TotalEmpleadosEncontrados = 0,
                TotalPlanillasGeneradas = 0,
                TotalPlanillasOmitidasPorExistir = 0,
                TotalMontoNetoGenerado = 0m,
                Mensaje = esTodos
                    ? "No se encontraron empleados activos en el sistema."
                    : $"No se encontraron empleados activos en el departamento '{request.Departamento}'."
            };
        }

        // 2. Consulta fecha de corte del periodo (si existe)
        var periodoCierre = await _context.PeriodosCierrePlanilla
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Periodo == request.PeriodoMesAnio, cancellationToken);

        // 3. Parámetros laborales
        var parametros = await _context.ParametrosLaborales
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var paramDict = parametros.ToDictionary(p => p.Clave.ToUpperInvariant(), p => p.Valor);
        var inssLaboralRate   = (paramDict.TryGetValue("INSS_LABORAL", out var inssL) ? inssL : 7.00m) / 100m;
        var inssPatronalRate  = (paramDict.TryGetValue("INSS_PATRONAL", out var inssP) ? inssP : 21.50m) / 100m;
        var inatecRate        = (paramDict.TryGetValue("INATEC", out var inat) ? inat : 2.00m) / 100m;
        var horasLaboralesMes = paramDict.TryGetValue("HORAS_LABORALES_MES", out var hMes) && hMes > 0 ? hMes : 240m;
        var tasaPrestaciones  = paramDict.TryGetValue("TASA_PRESTACIONES_MENSUAL", out var tPrest) ? tPrest : 2.5m;

        // 4. Feriados del período
        var inicioPeriodo = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var finPeriodo    = inicioPeriodo.AddMonths(1).AddTicks(-1);

        var feriadosPeriodo = await _context.DiasFeriados
            .AsNoTracking()
            .Where(f => f.Fecha >= inicioPeriodo && f.Fecha <= finPeriodo)
            .Select(f => f.Fecha.Date)
            .ToListAsync(cancellationToken);
        var feriadosSet = feriadosPeriodo.ToHashSet();

        // 5. Tramos IR
        var tramosIr = await _context.TablaImpuestoRenta
            .AsNoTracking()
            .Where(t => t.Activo && (t.AnioVigencia == anio || t.AnioVigencia == 2026))
            .OrderBy(t => t.DesdeMontoAnual)
            .ToListAsync(cancellationToken);

        // 6. Planillas existentes en el período
        var planillasExistentesIds = await _context.HistorialPlanillas
            .Where(p => p.PeriodoMesAnio == request.PeriodoMesAnio)
            .Select(p => p.IdEmpleado)
            .ToListAsync(cancellationToken);
        var planillasExistentesSet = planillasExistentesIds.ToHashSet();

        var planillasCreadas = new List<PlanillaResponseDto>();
        var totalOmitidas = 0;
        var totalNeto = 0m;

        foreach (var emp in empleados)
        {
            if (planillasExistentesSet.Contains(emp.IdEmpleado))
            {
                totalOmitidas++;
                continue;
            }

            // Horas extras
            var queryHorasExtras = _context.HorasExtras
                .Where(h => h.IdEmpleado == emp.IdEmpleado &&
                            h.Estado     == "Aprobado"     &&
                            h.Fecha.Year == anio           &&
                            h.Fecha.Month == mes);

            if (periodoCierre != null)
            {
                queryHorasExtras = queryHorasExtras.Where(h => h.Fecha <= periodoCierre.FechaCorteHorasExtras);
            }

            var horasExtrasList = await queryHorasExtras.ToListAsync(cancellationToken);
            var totalHorasExtras = horasExtrasList.Sum(h => h.CantidadHoras);
            var pagoHorasExtras  = horasExtrasList.Sum(h => h.MontoPagar);

            // Ingresos
            var salarioBase   = emp.SalarioBaseMensual;
            var totalIngresos = salarioBase + request.ComisionesGenerales + pagoHorasExtras + request.IncentivosGenerales;

            // Tardanzas
            var registrosTardanza = await _context.HistorialAsistencias
                .Where(h => h.IdEmpleado    == emp.IdEmpleado &&
                            h.Fecha         >= inicioPeriodo   &&
                            h.Fecha         <= finPeriodo       &&
                            h.MinutosTardanza > 0)
                .ToListAsync(cancellationToken);

            var tardanzasValidas = registrosTardanza
                .Where(h => !feriadosSet.Contains(h.Fecha.Date))
                .ToList();

            var totalMinutosTardanza = tardanzasValidas.Sum(h => h.MinutosTardanza);
            var valorPorMinuto       = (salarioBase > 0 && horasLaboralesMes > 0)
                ? salarioBase / horasLaboralesMes / 60m
                : 0m;
            var deduccionTardanza    = Math.Round(valorPorMinuto * totalMinutosTardanza, 2);

            // Deducciones
            var inssLaboral = Math.Round(totalIngresos * inssLaboralRate, 2);
            var irLaboral   = CalcularIrMensualDinamico(totalIngresos, tramosIr);
            var totalDeducciones = inssLaboral + irLaboral + deduccionTardanza + request.OtrasDeduccionesGenerales;
            var salarioNeto = totalIngresos - totalDeducciones;

            var inssPatronal = Math.Round(totalIngresos * inssPatronalRate, 2);
            var inatec       = Math.Round(totalIngresos * inatecRate, 2);
            var prestacion   = Math.Round(salarioBase / 30m * tasaPrestaciones, 2);

            var planilla = new HistorialPlanilla
            {
                IdEmpleado         = emp.IdEmpleado,
                PeriodoMesAnio     = request.PeriodoMesAnio,
                SalarioBase        = salarioBase,
                TotalHorasExtras   = totalHorasExtras,
                PagoHorasExtras    = pagoHorasExtras,
                SalarioBruto       = totalIngresos,
                InssLaboral        = inssLaboral,
                IrLaboral          = irLaboral,
                MinutosTardanzaMes = totalMinutosTardanza,
                DeduccionTardanza  = deduccionTardanza,
                Embargo            = 0m,
                Sindicato          = 0m,
                OtrasDeducciones   = request.OtrasDeduccionesGenerales,
                TotalDeducciones   = totalDeducciones,
                SalarioNeto        = salarioNeto,
                AcumuladoAguinaldo = prestacion,
                FechaEmision       = DateTime.UtcNow
            };

            _context.HistorialPlanillas.Add(planilla);
            totalNeto += salarioNeto;

            planillasCreadas.Add(new PlanillaResponseDto
            {
                IdEmpleado             = emp.IdEmpleado,
                NombreEmpleado         = emp.Nombres + " " + emp.Apellidos,
                CargoEmpleado          = emp.CargoFuncion,
                Departamento           = emp.Departamento,
                PeriodoMesAnio         = request.PeriodoMesAnio,
                SalarioBase            = salarioBase,
                Comisiones             = request.ComisionesGenerales,
                TotalHorasExtras       = totalHorasExtras,
                PagoHorasExtras        = pagoHorasExtras,
                Incentivos             = request.IncentivosGenerales,
                TotalIngresos          = totalIngresos,
                InssLaboral            = inssLaboral,
                IrLaboral              = irLaboral,
                DeduccionTardanza      = deduccionTardanza,
                MinutosTardanzaMes     = totalMinutosTardanza,
                Embargo                = 0m,
                Sindicato              = 0m,
                OtrasDeducciones       = request.OtrasDeduccionesGenerales,
                TotalDeducciones       = totalDeducciones,
                SalarioNeto            = salarioNeto,
                InssPatronal           = inssPatronal,
                Inatec                 = inatec,
                AcumuladoVacaciones    = prestacion,
                AcumuladoAguinaldo     = prestacion,
                AcumuladoIndemnizacion = prestacion,
                FechaEmision           = planilla.FechaEmision
            });
        }

        if (planillasCreadas.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
            // Sincronizar IDs generados
            for (int i = 0; i < planillasCreadas.Count; i++)
            {
                // El contexto actualiza los IdPlanilla tras SaveChangesAsync
            }
        }

        var depNombre = esTodos ? "todos los departamentos" : $"departamento '{request.Departamento}'";
        var mensaje = $"Se generaron {planillasCreadas.Count} planillas para {depNombre} en el período {request.PeriodoMesAnio}.";
        if (totalOmitidas > 0)
        {
            mensaje += $" Se omitieron {totalOmitidas} empleados que ya tenían planilla generada.";
        }

        return new GenerarPlanillaPorDepartamentoResponseDto
        {
            PeriodoMesAnio = request.PeriodoMesAnio,
            Departamento = esTodos ? "Todos" : request.Departamento!,
            TotalEmpleadosEncontrados = empleados.Count,
            TotalPlanillasGeneradas = planillasCreadas.Count,
            TotalPlanillasOmitidasPorExistir = totalOmitidas,
            TotalMontoNetoGenerado = totalNeto,
            Planillas = planillasCreadas,
            Mensaje = mensaje
        };
    }

    private static decimal CalcularIrMensualDinamico(decimal ingresoMensual, List<TablaImpuestoRenta> tramos)
    {
        var anual = ingresoMensual * 12m;

        if (tramos != null && tramos.Count > 0)
        {
            var tramoCoincidente = tramos
                .FirstOrDefault(t => anual >= t.DesdeMontoAnual &&
                                     (!t.HastaMontoAnual.HasValue || anual <= t.HastaMontoAnual.Value));

            if (tramoCoincidente != null)
            {
                var exceso = Math.Max(0m, anual - tramoCoincidente.MontoBaseExceso);
                var irAnual = tramoCoincidente.CuotaFija + (exceso * tramoCoincidente.PorcentajeAplicable);
                return Math.Round(irAnual / 12m, 2);
            }
        }

        // Fallback por ley 822 si no hay tramos en BD
        decimal fallbackAnual;
        if (anual <= 100_000m)
            fallbackAnual = 0m;
        else if (anual <= 200_000m)
            fallbackAnual = (anual - 100_000m) * 0.15m;
        else if (anual <= 350_000m)
            fallbackAnual = (anual - 200_000m) * 0.20m + 15_000m;
        else if (anual <= 500_000m)
            fallbackAnual = (anual - 350_000m) * 0.25m + 45_000m;
        else
            fallbackAnual = (anual - 500_000m) * 0.30m + 82_500m;

        return Math.Round(fallbackAnual / 12m, 2);
    }
}
