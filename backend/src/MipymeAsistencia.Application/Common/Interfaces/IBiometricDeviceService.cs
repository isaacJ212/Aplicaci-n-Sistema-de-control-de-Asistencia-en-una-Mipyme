using MipymeAsistencia.Application.Common.DTOs.Biometrico;
using MipymeAsistencia.Domain.Entities;

namespace MipymeAsistencia.Application.Common.Interfaces;

public interface IBiometricDeviceService
{
    Task<bool> ProbarConexionAsync(DispositivoBiometrico dispositivo, CancellationToken cancellationToken = default);
    Task<ResultadoSincronizacionDto> SincronizarDispositivoAsync(int idDispositivo, CancellationToken cancellationToken = default);
    Task<ResultadoSincronizacionDto> SincronizarTodosDispositivosAsync(CancellationToken cancellationToken = default);
    Task<ResultadoSincronizacionDto> IngestarLoteMarcajesAsync(int idDispositivo, List<MarcajeBiometricoItemDto> marcajes, CancellationToken cancellationToken = default);
}
