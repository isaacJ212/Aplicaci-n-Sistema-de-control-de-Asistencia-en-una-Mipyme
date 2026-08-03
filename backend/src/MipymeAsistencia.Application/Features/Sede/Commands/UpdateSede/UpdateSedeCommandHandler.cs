using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Sede;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Sede.Commands.UpdateSede;

public class UpdateSedeCommandHandler : IRequestHandler<UpdateSedeCommand, SedeResponseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSedeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SedeResponseDto> Handle(UpdateSedeCommand request, CancellationToken cancellationToken)
    {
        var sede = await _context.ConfiguracionesSede
            .FirstOrDefaultAsync(cancellationToken);

        if (sede is null)
            throw new KeyNotFoundException("No existe una configuración de sede registrada.");

        // Parsea las horas en formato HH:mm — el validator garantiza el formato correcto
        var entrada = TimeSpan.Parse(request.HoraEntradaOficial);
        var salida  = TimeSpan.Parse(request.HoraSalidaOficial);

        sede.NombreSede              = request.NombreSede;
        sede.LatitudSede             = request.LatitudSede;
        sede.LongitudSede            = request.LongitudSede;
        sede.RadioToleranciaMetros   = request.RadioToleranciaMetros;
        sede.HoraEntradaOficial      = entrada;
        sede.HoraSalidaOficial       = salida;
        sede.DuracionAlmuerzoMinutos = request.DuracionAlmuerzoMinutos;

        await _context.SaveChangesAsync(cancellationToken);

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
