using System.Security.Cryptography;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.ValidarQr;

public class ValidarQrCommandHandler : IRequestHandler<ValidarQrCommand, ValidarQrResponseDto>
{
    private readonly IApplicationDbContext _context;

    public ValidarQrCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidarQrResponseDto> Handle(ValidarQrCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
            throw new KeyNotFoundException("No existe configuración de sede registrada.");

        if (string.IsNullOrWhiteSpace(request.TokenQrEscaneado) || request.TokenQrEscaneado != sede.TokenQrActual)
            throw new InvalidOperationException("El QR escaneado no es válido o ya expiró.");

        var validacion = await _context.ValidacionesQrMarcaje
            .Where(v => v.IdEmpleado == request.IdEmpleado && v.TokenQrEscaneado == request.TokenQrEscaneado)
            .OrderByDescending(v => v.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (validacion is null)
            throw new KeyNotFoundException("No existe una validación de QR para este empleado.");

        if (validacion.FechaExpiracion < DateTime.UtcNow)
            throw new InvalidOperationException("El QR ya expiró. Solicita uno nuevo.");

        if (validacion.FueUtilizado)
            throw new InvalidOperationException("Este QR ya fue usado.");

        var codigoOtp = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6");

        validacion.CodigoOtpGenerado = codigoOtp;
        validacion.FueUtilizado = true;
        validacion.IntentosFallidos = 0;

        await _context.SaveChangesAsync(cancellationToken);

        return new ValidarQrResponseDto
        {
            EsValido = true,
            CodigoOtpGenerado = codigoOtp,
            FechaExpiracion = validacion.FechaExpiracion,
            Mensaje = "QR validado correctamente."
        };
    }
}
