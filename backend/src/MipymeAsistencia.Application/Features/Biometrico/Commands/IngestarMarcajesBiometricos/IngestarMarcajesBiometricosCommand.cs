using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Application.Common.Interfaces;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.IngestarMarcajesBiometricos;

public class IngestarMarcajesBiometricosCommand : IRequest<ResultadoSincronizacionDto>
{
    public int IdDispositivo { get; set; }
    public List<MarcajeBiometricoItemDto> Marcajes { get; set; } = new();
}

public class IngestarMarcajesBiometricosCommandHandler : IRequestHandler<IngestarMarcajesBiometricosCommand, ResultadoSincronizacionDto>
{
    private readonly IBiometricDeviceService _deviceService;

    public IngestarMarcajesBiometricosCommandHandler(IBiometricDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task<ResultadoSincronizacionDto> Handle(IngestarMarcajesBiometricosCommand request, CancellationToken cancellationToken)
    {
        return await _deviceService.IngestarLoteMarcajesAsync(request.IdDispositivo, request.Marcajes, cancellationToken);
    }
}
