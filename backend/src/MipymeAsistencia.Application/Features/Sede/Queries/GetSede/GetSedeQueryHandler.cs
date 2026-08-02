using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Sede;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Sede.Queries.GetSede;

public class GetSedeQueryHandler : IRequestHandler<GetSedeQuery, SedeResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetSedeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SedeResponseDto> Handle(GetSedeQuery request, CancellationToken cancellationToken)
    {
        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
            throw new KeyNotFoundException("No existe una configuración de sede registrada.");

        return new SedeResponseDto
        {
            IdSede                  = sede.IdSede,
            NombreSede              = sede.NombreSede,
            LatitudSede             = sede.LatitudSede,
            LongitudSede            = sede.LongitudSede,
            RadioToleranciaMetros   = sede.RadioToleranciaMetros,
            HoraEntradaOficial      = sede.HoraEntradaOficial.ToString(@"hh\:mm"),
            HoraSalidaOficial       = sede.HoraSalidaOficial.ToString(@"hh\:mm"),
            DuracionAlmuerzoMinutos = sede.DuracionAlmuerzoMinutos,
            TokenQrActual           = sede.TokenQrActual,
            QrUltimaActualizacion   = sede.QrUltimaActualizacion
        };
    }
}
