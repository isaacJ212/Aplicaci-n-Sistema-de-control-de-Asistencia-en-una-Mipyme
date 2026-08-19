using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Queries.GetDispositivoBiometricoById;

public class GetDispositivoBiometricoByIdQuery : IRequest<DispositivoBiometricoDto>
{
    public int IdDispositivo { get; set; }
}

public class GetDispositivoBiometricoByIdQueryHandler : IRequestHandler<GetDispositivoBiometricoByIdQuery, DispositivoBiometricoDto>
{
    private readonly IApplicationDbContext _context;

    public GetDispositivoBiometricoByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DispositivoBiometricoDto> Handle(GetDispositivoBiometricoByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _context.DispositivosBiometricos
            .Include(x => x.RegistrosMarcajes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdDispositivo == request.IdDispositivo, cancellationToken);

        if (d is null)
            throw new KeyNotFoundException($"Dispositivo biométrico #{request.IdDispositivo} no encontrado.");

        return new DispositivoBiometricoDto
        {
            IdDispositivo            = d.IdDispositivo,
            NombreDispositivo        = d.NombreDispositivo,
            DireccionIp              = d.DireccionIp,
            Puerto                   = d.Puerto,
            TipoProtocolo            = d.TipoProtocolo,
            Ubicacion                = d.Ubicacion,
            Activo                   = d.Activo,
            UltimaSincronizacion     = d.UltimaSincronizacion,
            EstadoConexion           = d.EstadoConexion,
            TotalMarcajesAlmacenados = d.RegistrosMarcajes.Count
        };
    }
}
