using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Queries.GetDispositivosBiometricos;

public class GetDispositivosBiometricosQuery : IRequest<List<DispositivoBiometricoDto>>
{
}

public class GetDispositivosBiometricosQueryHandler : IRequestHandler<GetDispositivosBiometricosQuery, List<DispositivoBiometricoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDispositivosBiometricosQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<DispositivoBiometricoDto>> Handle(GetDispositivosBiometricosQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.DispositivosBiometricos
            .Include(d => d.RegistrosMarcajes)
            .AsNoTracking()
            .OrderBy(d => d.IdDispositivo)
            .ToListAsync(cancellationToken);

        return list.Select(d => new DispositivoBiometricoDto
        {
            IdDispositivo              = d.IdDispositivo,
            NombreDispositivo          = d.NombreDispositivo,
            DireccionIp                = d.DireccionIp,
            Puerto                     = d.Puerto,
            TipoProtocolo              = d.TipoProtocolo,
            Ubicacion                  = d.Ubicacion,
            Activo                     = d.Activo,
            UltimaSincronizacion       = d.UltimaSincronizacion,
            EstadoConexion             = d.EstadoConexion,
            TotalMarcajesAlmacenados   = d.RegistrosMarcajes.Count
        }).ToList();
    }
}
