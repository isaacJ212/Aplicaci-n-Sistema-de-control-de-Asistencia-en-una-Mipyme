using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.RegistrarAsistencia;

/// <summary>
/// Marcaje inteligente: un solo endpoint determina la acción según el historial del día.
///
///   Sin registro hoy         → Entrada
///   Entrada sin almuerzo      → Inicio de Almuerzo
///   Inicio almuerzo sin fin   → Fin de Almuerzo
///   Almuerzo completo         → Salida
///
/// La validación OTP ya no es bloqueante — si el token QR es válido se procede.
/// Si tokenQrEscaneado llega vacío se omite la verificación (útil para marcaje manual).
/// </summary>
public class RegistrarAsistenciaCommandHandler
    : IRequestHandler<RegistrarAsistenciaCommand, AsistenciaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public RegistrarAsistenciaCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<AsistenciaResponseDto> Handle(
        RegistrarAsistenciaCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Verificar empleado ──────────────────────────────────────────
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken)
            ?? throw new KeyNotFoundException("El empleado no existe.");

        // ── 2. Verificar sede ──────────────────────────────────────────────
        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("No existe configuración de sede registrada.");

        // ── 3. Validar token QR (no bloqueante si viene vacío) ─────────────
        if (!string.IsNullOrWhiteSpace(request.TokenQrEscaneado)
            && !string.IsNullOrWhiteSpace(sede.TokenQrActual)
            && request.TokenQrEscaneado != sede.TokenQrActual)
        {
            throw new InvalidOperationException(
                "El token QR no coincide con el activo de la sede. Escanea el QR actual.");
        }

        // ── 4. Calcular distancia GPS y validar geocerca ───────────────────
        var distancia = CalcularDistanciaEnMetros(
            request.LatitudMarcaje,  request.LongitudMarcaje,
            sede.LatitudSede,        sede.LongitudSede);

        var enRango = distancia <= sede.RadioToleranciaMetros;

        if (!enRango)
        {
            throw new ArgumentException(
                $"Estás fuera de la zona permitida. Distancia: {Math.Round(distancia, 0)}m · Radio permitido: {sede.RadioToleranciaMetros}m. Acercate a la sede para registrar.");
        }

        // ── 5. Buscar registro de asistencia de hoy ────────────────────────
        var fechaHoy = DateOnly.FromDateTime(DateTime.UtcNow);
        // La columna fecha es timestamp — convertimos a DateTime para comparar
        var inicioDia = fechaHoy.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var finDia    = fechaHoy.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var asistencia = await _context.HistorialAsistencias
            .FirstOrDefaultAsync(h =>
                h.IdEmpleado == request.IdEmpleado &&
                h.Fecha >= inicioDia && h.Fecha <= finDia,
                cancellationToken);

        var ahora    = DateTime.UtcNow.TimeOfDay;
        var mensaje  = "";

        if (asistencia is null)
        {
            // ── ENTRADA ────────────────────────────────────────────────────
            var estadoEntrada  = "A Tiempo";
            var minutosTardanza = 0;

            // Comparar con la hora oficial + margen de tolerancia de la sede
            var horaLimiteTolerancia = sede.HoraEntradaOficial.Add(TimeSpan.FromMinutes(sede.MinutosTolerancia));
            if (ahora > horaLimiteTolerancia)
            {
                var tardanza = ahora - sede.HoraEntradaOficial;
                minutosTardanza = (int)tardanza.TotalMinutes;
                estadoEntrada   = "Tardanza";
            }
            else if (ahora > sede.HoraEntradaOficial)
            {
                // Entra dentro de la tolerancia — se marca A Tiempo, con nota informativa
                minutosTardanza = 0;
                estadoEntrada   = "A Tiempo";
            }

            asistencia = new HistorialAsistencia
            {
                IdEmpleado               = request.IdEmpleado,
                Fecha                    = inicioDia,
                HoraEntrada              = ahora,
                LatitudMarcaje           = request.LatitudMarcaje,
                LongitudMarcaje          = request.LongitudMarcaje,
                DistanciaCalculadaMetros = (decimal)distancia,
                EstadoAsistencia         = estadoEntrada,
                MinutosTardanza          = minutosTardanza,
                EstaDentroDelRangoGps    = enRango,
            };

            _context.HistorialAsistencias.Add(asistencia);
            mensaje = minutosTardanza > 0
                ? $"Entrada registrada con {minutosTardanza} min de tardanza."
                : "Entrada registrada a tiempo. ¡Buen día!";
        }
        else if (asistencia.InicioAlmuerzo is null)
        {
            // ── INICIO DE ALMUERZO ─────────────────────────────────────────
            asistencia.InicioAlmuerzo = ahora;
            mensaje = "Inicio de almuerzo registrado.";
        }
        else if (asistencia.FinAlmuerzo is null)
        {
            // ── FIN DE ALMUERZO ────────────────────────────────────────────
            asistencia.FinAlmuerzo = ahora;
            mensaje = "Fin de almuerzo registrado. ¡Bienvenido de vuelta!";
        }
        else if (asistencia.HoraSalida is null)
        {
            // ── SALIDA ─────────────────────────────────────────────────────
            asistencia.HoraSalida = ahora;
            mensaje = "Salida registrada correctamente. ¡Hasta mañana!";
        }
        else
        {
            throw new InvalidOperationException(
                "Ya completaste todos los marcajes del día (Entrada, Almuerzo y Salida).");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(asistencia, mensaje);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AsistenciaResponseDto MapToDto(HistorialAsistencia a, string mensaje) => new()
    {
        IdAsistencia             = a.IdAsistencia,
        IdEmpleado               = a.IdEmpleado,
        Fecha                    = a.Fecha,
        HoraEntrada              = a.HoraEntrada.ToString(@"hh\:mm"),
        InicioAlmuerzo           = a.InicioAlmuerzo?.ToString(@"hh\:mm"),
        FinAlmuerzo              = a.FinAlmuerzo?.ToString(@"hh\:mm"),
        HoraSalida               = a.HoraSalida?.ToString(@"hh\:mm"),
        LatitudMarcaje           = a.LatitudMarcaje,
        LongitudMarcaje          = a.LongitudMarcaje,
        DistanciaCalculadaMetros = a.DistanciaCalculadaMetros,
        EstadoAsistencia         = a.EstadoAsistencia,
        MinutosTardanza          = a.MinutosTardanza,
        EstaDentroDelRangoGps    = a.EstaDentroDelRangoGps,
        Mensaje                  = mensaje,
    };

    private static double CalcularDistanciaEnMetros(
        decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
        var dLon = (double)(lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos((double)lat1 * Math.PI / 180.0)
              * Math.Cos((double)lat2 * Math.PI / 180.0)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 6_371_000 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
