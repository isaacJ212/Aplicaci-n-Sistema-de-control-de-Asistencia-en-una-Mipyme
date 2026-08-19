using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.ProbarConexionDispositivo;

public class ProbarConexionDispositivoCommand : IRequest<bool>
{
    public int IdDispositivo { get; set; }
}

public class ProbarConexionDispositivoCommandHandler : IRequestHandler<ProbarConexionDispositivoCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IBiometricDeviceService _deviceService;

    public ProbarConexionDispositivoCommandHandler(IApplicationDbContext context, IBiometricDeviceService deviceService)
    {
        _context = context;
        _deviceService = deviceService;
    }

    public async Task<bool> Handle(ProbarConexionDispositivoCommand request, CancellationToken cancellationToken)
    {
        var d = await _context.DispositivosBiometricos
            .FirstOrDefaultAsync(x => x.IdDispositivo == request.IdDispositivo, cancellationToken);

        if (d is null)
            throw new KeyNotFoundException($"Dispositivo biométrico #{request.IdDispositivo} no encontrado.");

        return await _deviceService.ProbarConexionAsync(d, cancellationToken);
    }
}
