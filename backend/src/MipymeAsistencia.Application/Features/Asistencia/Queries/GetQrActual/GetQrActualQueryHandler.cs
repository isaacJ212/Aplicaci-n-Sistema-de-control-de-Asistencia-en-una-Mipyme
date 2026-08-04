using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetQrActual;

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
            throw new KeyNotFoundException("No existe configuración de sede registrada.");

        if (string.IsNullOrWhiteSpace(sede.TokenQrActual))
            throw new InvalidOperationException("Todavía no se ha generado un QR activo para la sede.");

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
