using System.Net.Sockets;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MipymeAsistencia.Infrastructure.Services;

public class BiometricDeviceService : IBiometricDeviceService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<BiometricDeviceService> _logger;

    public BiometricDeviceService(IApplicationDbContext context, ILogger<BiometricDeviceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ProbarConexionAsync(DispositivoBiometrico dispositivo, CancellationToken cancellationToken = default)
    {
        if (dispositivo.TipoProtocolo.Equals("Virtual_Mock", StringComparison.OrdinalIgnoreCase))
        {
            dispositivo.EstadoConexion = "Conectado";
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(dispositivo.DireccionIp, dispositivo.Puerto);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == connectTask && client.Connected)
            {
                dispositivo.EstadoConexion = "Conectado";
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            dispositivo.EstadoConexion = "Desconectado";
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al probar conexión con dispositivo biométrico {Ip}:{Puerto}", dispositivo.DireccionIp, dispositivo.Puerto);
            dispositivo.EstadoConexion = "Error";
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    public async Task<ResultadoSincronizacionDto> SincronizarDispositivoAsync(int idDispositivo, CancellationToken cancellationToken = default)
    {
        var resultado = new ResultadoSincronizacionDto { TotalDispositivosProcesados = 1 };

        var dispositivo = await _context.DispositivosBiometricos
            .FirstOrDefaultAsync(d => d.IdDispositivo == idDispositivo, cancellationToken);

        if (dispositivo == null)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Dispositivo biométrico #{idDispositivo} no encontrado.";
            resultado.TotalErrores++;
            return resultado;
        }

        if (!dispositivo.Activo)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"El dispositivo '{dispositivo.NombreDispositivo}' está inactivo.";
            return resultado;
        }

        // Probar conexión
        var conectado = await ProbarConexionAsync(dispositivo, cancellationToken);
        if (!conectado && !dispositivo.TipoProtocolo.Equals("Virtual_Mock", StringComparison.OrdinalIgnoreCase))
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"No se pudo establecer conexión con el reloj biométrico en {dispositivo.DireccionIp}:{dispositivo.Puerto}.";
            resultado.TotalErrores++;
            return resultado;
        }

        // Para protocolos de hardware simulado o pull estándar:
        // Si hay registros no procesados previamente en la tabla cruda, procesarlos
        var pendientes = await _context.RegistrosMarcajesBiometricos
            .Where(r => r.IdDispositivo == idDispositivo && !r.Procesado)
            .OrderBy(r => r.FechaHora)
            .ToListAsync(cancellationToken);

        resultado.TotalMarcajesLeidos = pendientes.Count;

        if (pendientes.Count > 0)
        {
            await ProcesarLoteRegistrosAsync(pendientes, resultado, cancellationToken);
        }
        else
        {
            resultado.Detalles.Add($"Dispositivo '{dispositivo.NombreDispositivo}' verificado. Sin marcajes pendientes por procesar.");
        }

        dispositivo.UltimaSincronizacion = DateTime.UtcNow;
        dispositivo.EstadoConexion = "Sincronizado";
        await _context.SaveChangesAsync(cancellationToken);

        resultado.Exitoso = resultado.TotalErrores == 0;
        resultado.Mensaje = $"Sincronización finalizada para '{dispositivo.NombreDispositivo}'. {resultado.TotalAsistenciasGeneradas} marcajes procesados correctamente.";
        return resultado;
    }

    public async Task<ResultadoSincronizacionDto> SincronizarTodosDispositivosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new ResultadoSincronizacionDto();

        var dispositivos = await _context.DispositivosBiometricos
            .Where(d => d.Activo)
            .ToListAsync(cancellationToken);

        resultado.TotalDispositivosProcesados = dispositivos.Count;

        foreach (var disp in dispositivos)
        {
            try
            {
                var res = await SincronizarDispositivoAsync(disp.IdDispositivo, cancellationToken);
                resultado.TotalMarcajesLeidos += res.TotalMarcajesLeidos;
                resultado.TotalMarcajesNuevos += res.TotalMarcajesNuevos;
                resultado.TotalAsistenciasGeneradas += res.TotalAsistenciasGeneradas;
                resultado.TotalErrores += res.TotalErrores;
                resultado.Detalles.AddRange(res.Detalles);
            }
            catch (Exception ex)
            {
                resultado.TotalErrores++;
                resultado.Detalles.Add($"Error en dispositivo '{disp.NombreDispositivo}': {ex.Message}");
            }
        }

        resultado.Exitoso = resultado.TotalErrores == 0;
        resultado.Mensaje = $"Sincronización global completada en {dispositivos.Count} dispositivos. Total asistencias generadas: {resultado.TotalAsistenciasGeneradas}.";
        return resultado;
    }

    public async Task<ResultadoSincronizacionDto> IngestarLoteMarcajesAsync(int idDispositivo, List<MarcajeBiometricoItemDto> marcajes, CancellationToken cancellationToken = default)
    {
        var resultado = new ResultadoSincronizacionDto
        {
            TotalDispositivosProcesados = 1,
            TotalMarcajesLeidos = marcajes.Count
        };

        var dispositivo = await _context.DispositivosBiometricos
            .FirstOrDefaultAsync(d => d.IdDispositivo == idDispositivo, cancellationToken);

        if (dispositivo == null)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Dispositivo biométrico #{idDispositivo} no encontrado.";
            resultado.TotalErrores++;
            return resultado;
        }

        var nuevosRegistros = new List<RegistroMarcajeBiometrico>();

        foreach (var item in marcajes)
        {
            var fechaUtc = item.FechaHora.Kind == DateTimeKind.Utc
                ? item.FechaHora
                : item.FechaHora.ToUniversalTime();

            // Evitar duplicados exactos
            var existe = await _context.RegistrosMarcajesBiometricos
                .AnyAsync(r => r.IdDispositivo == idDispositivo &&
                               r.NumeroEnrollamiento == item.NumeroEnrollamiento.Trim() &&
                               r.FechaHora == fechaUtc, cancellationToken);

            if (existe) continue;

            var reg = new RegistroMarcajeBiometrico
            {
                IdDispositivo        = idDispositivo,
                NumeroEnrollamiento  = item.NumeroEnrollamiento.Trim(),
                FechaHora            = fechaUtc,
                TipoMarcaje          = item.TipoMarcaje,
                TipoVerificacion     = string.IsNullOrWhiteSpace(item.TipoVerificacion) ? "Huella" : item.TipoVerificacion.Trim(),
                Procesado            = false
            };

            _context.RegistrosMarcajesBiometricos.Add(reg);
            nuevosRegistros.Add(reg);
        }

        await _context.SaveChangesAsync(cancellationToken);
        resultado.TotalMarcajesNuevos = nuevosRegistros.Count;

        if (nuevosRegistros.Count > 0)
        {
            await ProcesarLoteRegistrosAsync(nuevosRegistros, resultado, cancellationToken);
        }

        dispositivo.UltimaSincronizacion = DateTime.UtcNow;
        dispositivo.EstadoConexion = "Sincronizado";
        await _context.SaveChangesAsync(cancellationToken);

        resultado.Exitoso = resultado.TotalErrores == 0;
        resultado.Mensaje = $"Lote procesado exitosamente: {resultado.TotalMarcajesNuevos} registros ingestados, {resultado.TotalAsistenciasGeneradas} asistencias sincronizadas.";
        return resultado;
    }

    private async Task ProcesarLoteRegistrosAsync(List<RegistroMarcajeBiometrico> registros, ResultadoSincronizacionDto resultado, CancellationToken cancellationToken)
    {
        var sede = await _context.ConfiguracionesSede.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
                   ?? new ConfiguracionSede();

        var empleados = await _context.Empleados.AsNoTracking().ToListAsync(cancellationToken);
        var feriados = await _context.DiasFeriados.AsNoTracking().ToListAsync(cancellationToken);
        var feriadosSet = feriados.Select(f => f.Fecha.Date).ToHashSet();

        foreach (var reg in registros)
        {
            try
            {
                // Mapear empleado por Cédula, INSS o ID
                var emp = empleados.FirstOrDefault(e =>
                    e.CedulaIdentificacion.Equals(reg.NumeroEnrollamiento, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(e.NumeroInss) && e.NumeroInss.Equals(reg.NumeroEnrollamiento, StringComparison.OrdinalIgnoreCase)) ||
                    e.IdEmpleado.ToString() == reg.NumeroEnrollamiento);

                if (emp == null)
                {
                    reg.Procesado = true;
                    reg.FechaProcesado = DateTime.UtcNow;
                    reg.ErrorProcesamiento = $"No se encontró empleado vinculado al enrollamiento '{reg.NumeroEnrollamiento}'.";
                    resultado.TotalErrores++;
                    resultado.Detalles.Add($"Enrollamiento desconocido: '{reg.NumeroEnrollamiento}'.");
                    continue;
                }

                var fechaPunto = reg.FechaHora.Date;
                var horaPunto  = reg.FechaHora.TimeOfDay;

                var asistencia = await _context.HistorialAsistencias
                    .FirstOrDefaultAsync(h => h.IdEmpleado == emp.IdEmpleado && h.Fecha.Date == fechaPunto, cancellationToken);

                var esFeriado = feriadosSet.Contains(fechaPunto);

                if (asistencia == null)
                {
                    // ── Marcaje de Entrada ─────────────────────────────
                    var estado = "A Tiempo";
                    var minutosTardanza = 0;

                    if (!esFeriado)
                    {
                        var horaLimite = sede.HoraEntradaOficial.Add(TimeSpan.FromMinutes(sede.MinutosTolerancia));
                        if (horaPunto > horaLimite)
                        {
                            estado = "Tardanza";
                            minutosTardanza = (int)(horaPunto - sede.HoraEntradaOficial).TotalMinutes;
                        }
                    }

                    asistencia = new HistorialAsistencia
                    {
                        IdEmpleado               = emp.IdEmpleado,
                        Fecha                    = DateTime.SpecifyKind(fechaPunto, DateTimeKind.Utc),
                        HoraEntrada              = horaPunto,
                        LatitudMarcaje           = sede.LatitudSede,
                        LongitudMarcaje          = sede.LongitudSede,
                        DistanciaCalculadaMetros = 0m,
                        EstadoAsistencia         = estado,
                        MinutosTardanza          = minutosTardanza,
                        EstaDentroDelRangoGps    = true
                    };

                    _context.HistorialAsistencias.Add(asistencia);
                    await _context.SaveChangesAsync(cancellationToken);

                    reg.IdAsistenciaGenerada = asistencia.IdAsistencia;
                    reg.Procesado            = true;
                    reg.FechaProcesado       = DateTime.UtcNow;
                    resultado.TotalAsistenciasGeneradas++;
                }
                else
                {
                    // Determinar slot de marcaje inteligente
                    if (asistencia.InicioAlmuerzo == null && reg.TipoMarcaje == 2)
                    {
                        asistencia.InicioAlmuerzo = horaPunto;
                    }
                    else if (asistencia.FinAlmuerzo == null && (reg.TipoMarcaje == 3 || (asistencia.InicioAlmuerzo != null && horaPunto > asistencia.InicioAlmuerzo.Value)))
                    {
                        asistencia.FinAlmuerzo = horaPunto;
                    }
                    else if (asistencia.HoraSalida == null)
                    {
                        asistencia.HoraSalida = horaPunto;
                    }

                    reg.IdAsistenciaGenerada = asistencia.IdAsistencia;
                    reg.Procesado            = true;
                    reg.FechaProcesado       = DateTime.UtcNow;
                    resultado.TotalAsistenciasGeneradas++;
                }
            }
            catch (Exception ex)
            {
                reg.Procesado          = false;
                reg.ErrorProcesamiento = ex.Message;
                resultado.TotalErrores++;
                resultado.Detalles.Add($"Error al procesar marcaje ID {reg.IdRegistroBiometrico}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
