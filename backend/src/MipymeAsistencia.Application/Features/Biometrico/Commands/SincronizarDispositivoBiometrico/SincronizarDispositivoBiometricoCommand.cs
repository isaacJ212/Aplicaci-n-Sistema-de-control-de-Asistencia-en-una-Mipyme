using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.SincronizarDispositivoBiometrico;

public class SincronizarDispositivoBiometricoCommand : IRequest<ResultadoSincronizacionDto>
{
    public int? IdDispositivo { get; set; }
}

public class SincronizarDispositivoBiometricoCommandHandler : IRequestHandler<SincronizarDispositivoBiometricoCommand, ResultadoSincronizacionDto>
{
    private readonly IBiometricDeviceService _deviceService;

    public SincronizarDispositivoBiometricoCommandHandler(IBiometricDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task<ResultadoSincronizacionDto> Handle(SincronizarDispositivoBiometricoCommand request, CancellationToken cancellationToken)
    {
        if (request.IdDispositivo.HasValue && request.IdDispositivo.Value > 0)
        {
            return await _deviceService.SincronizarDispositivoAsync(request.IdDispositivo.Value, cancellationToken);
        }

        return await _deviceService.SincronizarTodosDispositivosAsync(cancellationToken);
    }
}
