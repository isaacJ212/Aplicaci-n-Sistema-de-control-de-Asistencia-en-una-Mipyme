using System.Security.Cryptography;
using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.GenerarQr;

public class GenerarQrCommandHandler : IRequestHandler<GenerarQrCommand, string>
{
    private readonly IApplicationDbContext _context;

    public GenerarQrCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GenerarQrCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException("El empleado no existe.");

        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
            throw new KeyNotFoundException("No existe configuración de sede registrada.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var ahora = DateTime.UtcNow;

        sede.TokenQrActual = token;
        sede.QrUltimaActualizacion = ahora;

        var validacion = new ValidacionQrMarcaje
        {
            IdEmpleado = empleado.IdEmpleado,
            CodigoOtpGenerado = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6"),
            TokenQrEscaneado = token,
            FechaCreacion = ahora,
            FechaExpiracion = ahora.AddSeconds(30),
            FueUtilizado = false,
            IntentosFallidos = 0
        };

        _context.ValidacionesQrMarcaje.Add(validacion);
        await _context.SaveChangesAsync(cancellationToken);

        return token;
    }
}
