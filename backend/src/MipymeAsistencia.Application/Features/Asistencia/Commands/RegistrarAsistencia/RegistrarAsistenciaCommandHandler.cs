using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.RegistrarAsistencia;

public class RegistrarAsistenciaCommandHandler : IRequestHandler<RegistrarAsistenciaCommand, AsistenciaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public RegistrarAsistenciaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AsistenciaResponseDto> Handle(RegistrarAsistenciaCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
            throw new KeyNotFoundException("No existe configuración de sede registrada.");

        var tokenValido = !string.IsNullOrWhiteSpace(request.TokenQrEscaneado)
            && request.TokenQrEscaneado == sede.TokenQrActual;

        if (!tokenValido)
            throw new InvalidOperationException("El token QR no coincide con la sede actual o ya expiró.");

        var validacion = await _context.ValidacionesQrMarcaje
            .Where(v => v.IdEmpleado == request.IdEmpleado && v.TokenQrEscaneado == request.TokenQrEscaneado)
            .OrderByDescending(v => v.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (validacion is null || validacion.CodigoOtpGenerado != request.CodigoOtpGenerado)
            throw new InvalidOperationException("El código OTP no es válido para este QR.");

        if (validacion.FechaExpiracion < DateTime.UtcNow)
            throw new InvalidOperationException("El código OTP ya expiró.");

        var fechaHoy = DateTime.UtcNow.Date;
        var asistenciaExistente = await _context.HistorialAsistencias
            .FirstOrDefaultAsync(h => h.IdEmpleado == request.IdEmpleado && h.Fecha == fechaHoy, cancellationToken);

        if (asistenciaExistente is not null)
            throw new InvalidOperationException("Ya existe un registro de asistencia para este empleado hoy.");

        var distancia = CalcularDistanciaEnMetros(
            request.LatitudMarcaje,
            request.LongitudMarcaje,
            sede.LatitudSede,
            sede.LongitudSede);

        var ahora = DateTime.UtcNow.TimeOfDay;
        var horaEntrada = TimeSpan.FromHours(8);
        var salidaOficial = sede.HoraSalidaOficial;
        var tardanza = new TimeSpan();

        var estado = "A Tiempo";
        if (ahora > horaEntrada)
        {
            tardanza = ahora - horaEntrada;
            estado = "Tardanza";
        }

        var nuevaAsistencia = new HistorialAsistencia
        {
            IdEmpleado = request.IdEmpleado,
            Fecha = fechaHoy,
            HoraEntrada = ahora,
            InicioAlmuerzo = null,
            FinAlmuerzo = null,
            HoraSalida = null,
            LatitudMarcaje = request.LatitudMarcaje,
            LongitudMarcaje = request.LongitudMarcaje,
            DistanciaCalculadaMetros = (decimal)distancia,
            EstadoAsistencia = estado,
            MinutosTardanza = (int)tardanza.TotalMinutes,
            EstaDentroDelRangoGps = distancia <= sede.RadioToleranciaMetros
        };

        _context.HistorialAsistencias.Add(nuevaAsistencia);
        await _context.SaveChangesAsync(cancellationToken);

        return new AsistenciaResponseDto
        {
            IdAsistencia = nuevaAsistencia.IdAsistencia,
            IdEmpleado = nuevaAsistencia.IdEmpleado,
            Fecha = nuevaAsistencia.Fecha,
            HoraEntrada = nuevaAsistencia.HoraEntrada.ToString(@"hh\:mm"),
            InicioAlmuerzo = nuevaAsistencia.InicioAlmuerzo?.ToString(@"hh\:mm"),
            FinAlmuerzo = nuevaAsistencia.FinAlmuerzo?.ToString(@"hh\:mm"),
            HoraSalida = nuevaAsistencia.HoraSalida?.ToString(@"hh\:mm"),
            LatitudMarcaje = nuevaAsistencia.LatitudMarcaje,
            LongitudMarcaje = nuevaAsistencia.LongitudMarcaje,
            DistanciaCalculadaMetros = nuevaAsistencia.DistanciaCalculadaMetros,
            EstadoAsistencia = nuevaAsistencia.EstadoAsistencia,
            MinutosTardanza = nuevaAsistencia.MinutosTardanza,
            EstaDentroDelRangoGps = nuevaAsistencia.EstaDentroDelRangoGps,
            Mensaje = "Asistencia registrada correctamente."
        };
    }

    private static double CalcularDistanciaEnMetros(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
        var dLon = (double)(lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
          + Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0)
          * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return 6371000 * c;
    }
}
