using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.ActualizarDispositivoBiometrico;

public class ActualizarDispositivoBiometricoCommand : IRequest<DispositivoBiometricoDto>
{
    public int IdDispositivo { get; set; }
    public string NombreDispositivo { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string TipoProtocolo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public string? ClaveComunicacion { get; set; }
    public bool Activo { get; set; }
}

public class ActualizarDispositivoBiometricoCommandHandler : IRequestHandler<ActualizarDispositivoBiometricoCommand, DispositivoBiometricoDto>
{
    private readonly IApplicationDbContext _context;

    public ActualizarDispositivoBiometricoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DispositivoBiometricoDto> Handle(ActualizarDispositivoBiometricoCommand request, CancellationToken cancellationToken)
    {
        var d = await _context.DispositivosBiometricos
            .Include(x => x.RegistrosMarcajes)
            .FirstOrDefaultAsync(x => x.IdDispositivo == request.IdDispositivo, cancellationToken);

        if (d is null)
            throw new KeyNotFoundException($"Dispositivo biométrico #{request.IdDispositivo} no encontrado.");

        d.NombreDispositivo = request.NombreDispositivo.Trim();
        d.DireccionIp       = request.DireccionIp.Trim();
        d.Puerto            = request.Puerto > 0 ? request.Puerto : 4370;
        d.TipoProtocolo     = string.IsNullOrWhiteSpace(request.TipoProtocolo) ? "ZKTeco_Standalone" : request.TipoProtocolo.Trim();
        d.Ubicacion         = request.Ubicacion?.Trim();
        d.ClaveComunicacion = request.ClaveComunicacion?.Trim();
        d.Activo            = request.Activo;

        await _context.SaveChangesAsync(cancellationToken);

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
