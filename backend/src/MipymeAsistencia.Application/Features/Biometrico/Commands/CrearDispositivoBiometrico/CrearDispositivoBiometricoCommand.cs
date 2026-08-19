using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.CrearDispositivoBiometrico;

public class CrearDispositivoBiometricoCommand : IRequest<DispositivoBiometricoDto>
{
    public string NombreDispositivo { get; set; } = "Reloj Biométrico";
    public string DireccionIp { get; set; } = "192.168.1.201";
    public int Puerto { get; set; } = 4370;
    public string TipoProtocolo { get; set; } = "ZKTeco_Standalone";
    public string? Ubicacion { get; set; } = "Entrada Principal";
    public string? ClaveComunicacion { get; set; }
    public bool Activo { get; set; } = true;
}

public class CrearDispositivoBiometricoCommandHandler : IRequestHandler<CrearDispositivoBiometricoCommand, DispositivoBiometricoDto>
{
    private readonly IApplicationDbContext _context;

    public CrearDispositivoBiometricoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DispositivoBiometricoDto> Handle(CrearDispositivoBiometricoCommand request, CancellationToken cancellationToken)
    {
        var dispositivo = new DispositivoBiometrico
        {
            NombreDispositivo = request.NombreDispositivo.Trim(),
            DireccionIp       = request.DireccionIp.Trim(),
            Puerto            = request.Puerto > 0 ? request.Puerto : 4370,
            TipoProtocolo     = string.IsNullOrWhiteSpace(request.TipoProtocolo) ? "ZKTeco_Standalone" : request.TipoProtocolo.Trim(),
            Ubicacion         = request.Ubicacion?.Trim(),
            ClaveComunicacion = request.ClaveComunicacion?.Trim(),
            Activo            = request.Activo,
            EstadoConexion    = "Desconectado"
        };

        _context.DispositivosBiometricos.Add(dispositivo);
        await _context.SaveChangesAsync(cancellationToken);

        return new DispositivoBiometricoDto
        {
            IdDispositivo            = dispositivo.IdDispositivo,
            NombreDispositivo        = dispositivo.NombreDispositivo,
            DireccionIp              = dispositivo.DireccionIp,
            Puerto                   = dispositivo.Puerto,
            TipoProtocolo            = dispositivo.TipoProtocolo,
            Ubicacion                = dispositivo.Ubicacion,
            Activo                   = dispositivo.Activo,
            UltimaSincronizacion     = dispositivo.UltimaSincronizacion,
            EstadoConexion           = dispositivo.EstadoConexion,
            TotalMarcajesAlmacenados = 0
        };
    }
}
