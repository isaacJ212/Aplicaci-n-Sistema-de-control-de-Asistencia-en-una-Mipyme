using System.Security.Cryptography;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetQrActual;

/// <summary>
/// Obtiene el QR activo de la sede (ENDPOINT PÚBLICO — kioscos).
/// Si no existe configuración de sede, la crea con valores por defecto.
/// Si no hay token QR o expiró (>24h), genera uno nuevo automáticamente.
/// </summary>
public class GetQrActualQueryHandler : IRequestHandler<GetQrActualQuery, QrActualResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetQrActualQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QrActualResponseDto> Handle(GetQrActualQuery request, CancellationToken cancellationToken)
    {
        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
        {
            sede = new ConfiguracionSede
            {
                NombreSede = "Sede Principal",
                LatitudSede = 12.13500m,
                LongitudSede = -86.28000m,
                RadioToleranciaMetros = 200,
                HoraEntradaOficial = new TimeSpan(8, 0, 0),
                HoraSalidaOficial = new TimeSpan(17, 0, 0),
                DuracionAlmuerzoMinutos = 60,
                MinutosTolerancia = 10,
            };
            _context.ConfiguracionesSede.Add(sede);
        }

        var ahora = DateTime.UtcNow;
        var tokenViejoONulo = string.IsNullOrWhiteSpace(sede.TokenQrActual)
                              || sede.QrUltimaActualizacion is null
                              || (ahora - sede.QrUltimaActualizacion.Value).TotalHours >= 24;

        if (tokenViejoONulo)
        {
            sede.TokenQrActual = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            sede.QrUltimaActualizacion = ahora;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new QrActualResponseDto
        {
            IdSede = sede.IdSede,
            NombreSede = sede.NombreSede,
            TokenQrActual = sede.TokenQrActual,
            QrUltimaActualizacion = sede.QrUltimaActualizacion,
            RadioToleranciaMetros = sede.RadioToleranciaMetros
        };
    }
}
