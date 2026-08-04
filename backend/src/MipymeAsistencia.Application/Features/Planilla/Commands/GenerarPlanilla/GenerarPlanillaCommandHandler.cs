using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;

/// <summary>
/// Handler que genera la planilla mensual con todos los cálculos de la
/// legislación laboral nicaragüense:
///
/// Fuente: Planilla real "Rubí del Valle" - Mayo 2026 + Ley 185 + Ley 822 LCT
///
/// ─── INGRESOS ───────────────────────────────────────────────────────────────
///   TotalIngresos = SalarioBase + Comisiones + PagoHorasExtras + Incentivos
///
/// ─── DEDUCCIONES LABORALES ──────────────────────────────────────────────────
///   INSS Laboral  = TotalIngresos × 7%                        (Ley 539)
///   IR Laboral    = Proyección anual con tabla progresiva      (Ley 822 LCT)
///
///   Tabla IR 2026 (C$ anuales):
///     Hasta C$  100,000           → 0%
///     C$ 100,000.01 – 200,000     → 15% sobre el exceso de C$ 100,000
///     C$ 200,000.01 – 350,000     → 20% sobre el exceso de C$ 200,000  + C$ 15,000
///     C$ 350,000.01 – 500,000     → 25% sobre el exceso de C$ 350,000  + C$ 45,000
///     Más de C$ 500,000           → 30% sobre el exceso de C$ 500,000  + C$ 82,500
///
/// ─── APORTES PATRONALES (cargo a la empresa) ────────────────────────────────
///   INSS Patronal = TotalIngresos × 21.5%
///   INATEC        = TotalIngresos × 2%
///
/// ─── PROVISIÓN DE PRESTACIONES (mensual) ───────────────────────────────────
///   Vacaciones    = (SalarioBase / 30) × 2.5   → 2.5 días/mes = 30 días/año
///   Aguinaldo     = (SalarioBase / 30) × 2.5   → 13° mes = SalarioBase/12 * mes
///   Indemnización = (SalarioBase / 30) × 2.5
/// </summary>
public class GenerarPlanillaCommandHandler
    : IRequestHandler<GenerarPlanillaCommand, PlanillaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GenerarPlanillaCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<PlanillaResponseDto> Handle(
        GenerarPlanillaCommand request, CancellationToken cancellationToken)
    {
        // ── Validar que no exista planilla del mismo periodo para el empleado ──
        var yaExiste = await _context.HistorialPlanillas
            .AnyAsync(p => p.IdEmpleado    == request.IdEmpleado &&
                           p.PeriodoMesAnio == request.PeriodoMesAnio, cancellationToken);

        if (yaExiste)
            throw new InvalidOperationException(
                $"Ya existe una planilla generada para el periodo {request.PeriodoMesAnio}.");

        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException($"Empleado con id {request.IdEmpleado} no encontrado.");

        // ── Suma horas extras aprobadas del periodo ───────────────────────────
        // Extrae año y mes del formato YYYY-MM
        var partes = request.PeriodoMesAnio.Split('-');
        var anio   = int.Parse(partes[0]);
        var mes    = int.Parse(partes[1]);

        var horasExtrasAprobadas = await _context.HorasExtras
            .Where(h => h.IdEmpleado == request.IdEmpleado &&
                        h.Estado     == "Aprobado"          &&
                        h.Fecha.Year == anio                &&
                        h.Fecha.Month == mes)
            .ToListAsync(cancellationToken);

        var totalHorasExtras = horasExtrasAprobadas.Sum(h => h.CantidadHoras);
        var pagoHorasExtras  = horasExtrasAprobadas.Sum(h => h.MontoPagar);

        // ── INGRESOS ──────────────────────────────────────────────────────────
        var salarioBase   = empleado.SalarioBaseMensual;
        var totalIngresos = salarioBase
                          + request.Comisiones
                          + pagoHorasExtras
                          + request.Incentivos;

        // ── DEDUCCIÓN POR TARDANZA ────────────────────────────────────────────
        // Valor por minuto = SalarioBase / 240h / 60min
        // Deducción = valor por minuto × total minutos tardanza del periodo
        var inicioPeriodo = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var finPeriodo    = inicioPeriodo.AddMonths(1).AddTicks(-1);

        var registrosTardanza = await _context.HistorialAsistencias
            .Where(h => h.IdEmpleado    == request.IdEmpleado &&
                        h.Fecha         >= inicioPeriodo       &&
                        h.Fecha         <= finPeriodo           &&
                        h.MinutosTardanza > 0)
            .ToListAsync(cancellationToken);

        var totalMinutosTardanza = registrosTardanza.Sum(h => h.MinutosTardanza);
        var valorPorMinuto       = salarioBase > 0 ? salarioBase / 240m / 60m : 0m;
        var deduccionTardanza    = Math.Round(valorPorMinuto * totalMinutosTardanza, 2);

        // ── INSS LABORAL: 7% sobre total ingresos ─────────────────────────────
        var inssLaboral = Math.Round(totalIngresos * 0.07m, 2);

        // ── IR LABORAL: tabla progresiva Ley 822 LCT ─────────────────────────
        var irLaboral = CalcularIrMensual(totalIngresos);

        // ── TOTAL DEDUCCIONES ─────────────────────────────────────────────────
        var totalDeducciones = inssLaboral
                             + irLaboral
                             + deduccionTardanza       // ← deducción por llegadas tardías
                             + request.Embargo
                             + request.Sindicato
                             + request.OtrasDeducciones;

        // ── SALARIO NETO ──────────────────────────────────────────────────────
        var salarioNeto = totalIngresos - totalDeducciones;

        // ── APORTES PATRONALES (informativo) ──────────────────────────────────
        var inssPatronal = Math.Round(totalIngresos * 0.215m, 2);
        var inatec       = Math.Round(totalIngresos * 0.02m,  2);

        // ── PROVISIÓN PRESTACIONES (salario base / 30 * 2.5) ─────────────────
        var prestacion          = Math.Round(salarioBase / 30m * 2.5m, 2);
        var acumuladoAguinaldo  = prestacion;   // idéntico a vacaciones e indemnización

        // ── Persiste la planilla ───────────────────────────────────────────────
        var planilla = new HistorialPlanilla
        {
            IdEmpleado         = request.IdEmpleado,
            PeriodoMesAnio     = request.PeriodoMesAnio,
            SalarioBase        = salarioBase,
            TotalHorasExtras   = totalHorasExtras,
            PagoHorasExtras    = pagoHorasExtras,
            SalarioBruto       = totalIngresos,
            InssLaboral        = inssLaboral,
            IrLaboral          = irLaboral,
            // Otras deducciones incluye tardanza + embargo + sindicato + extras
            OtrasDeducciones   = deduccionTardanza + request.OtrasDeducciones + request.Embargo + request.Sindicato,
            TotalDeducciones   = totalDeducciones,
            SalarioNeto        = salarioNeto,
            AcumuladoAguinaldo = acumuladoAguinaldo,
            FechaEmision       = DateTime.UtcNow
        };

        _context.HistorialPlanillas.Add(planilla);
        await _context.SaveChangesAsync(cancellationToken);

        return new PlanillaResponseDto
        {
            IdPlanilla             = planilla.IdPlanilla,
            IdEmpleado             = planilla.IdEmpleado,
            NombreEmpleado         = empleado.Nombres + " " + empleado.Apellidos,
            CargoEmpleado          = empleado.CargoFuncion,
            PeriodoMesAnio         = planilla.PeriodoMesAnio,
            SalarioBase            = salarioBase,
            Comisiones             = request.Comisiones,
            TotalHorasExtras       = totalHorasExtras,
            PagoHorasExtras        = pagoHorasExtras,
            Incentivos             = request.Incentivos,
            TotalIngresos          = totalIngresos,
            InssLaboral            = inssLaboral,
            IrLaboral              = irLaboral,
            DeduccionTardanza      = deduccionTardanza,
            MinutosTardanzaMes     = totalMinutosTardanza,
            Embargo                = request.Embargo,
            Sindicato              = request.Sindicato,
            OtrasDeducciones       = request.OtrasDeducciones,
            TotalDeducciones       = totalDeducciones,
            SalarioNeto            = salarioNeto,
            InssPatronal           = inssPatronal,
            Inatec                 = inatec,
            AcumuladoVacaciones    = prestacion,
            AcumuladoAguinaldo     = acumuladoAguinaldo,
            AcumuladoIndemnizacion = prestacion,
            FechaEmision           = planilla.FechaEmision
        };
    }

    /// <summary>
    /// Calcula el IR mensual usando la tabla progresiva de la Ley 822 LCT Nicaragua.
    /// Se proyecta el ingreso mensual a anual, se aplica el tramo correspondiente
    /// y el resultado anual se divide entre 12.
    ///
    /// Tabla 2026 (C$ anuales):
    ///   0          – 100,000     →  0%
    ///   100,000.01 – 200,000     → 15% s/exceso 100,000
    ///   200,000.01 – 350,000     → 20% s/exceso 200,000  + 15,000
    ///   350,000.01 – 500,000     → 25% s/exceso 350,000  + 45,000
    ///   500,000.01 en adelante   → 30% s/exceso 500,000  + 82,500
    /// </summary>
    private static decimal CalcularIrMensual(decimal ingresoMensual)
    {
        var anual = ingresoMensual * 12m;
        decimal irAnual;

        if (anual <= 100_000m)
        {
            irAnual = 0m;
        }
        else if (anual <= 200_000m)
        {
            irAnual = (anual - 100_000m) * 0.15m;
        }
        else if (anual <= 350_000m)
        {
            irAnual = (anual - 200_000m) * 0.20m + 15_000m;
        }
        else if (anual <= 500_000m)
        {
            irAnual = (anual - 350_000m) * 0.25m + 45_000m;
        }
        else
        {
            irAnual = (anual - 500_000m) * 0.30m + 82_500m;
        }

        return Math.Round(irAnual / 12m, 2);
    }
}
