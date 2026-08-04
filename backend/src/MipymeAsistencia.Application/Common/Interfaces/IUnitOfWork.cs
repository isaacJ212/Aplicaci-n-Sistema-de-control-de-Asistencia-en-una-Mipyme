namespace MipymeAsistencia.Application.Common.Interfaces;

/// <summary>
/// Contrato del patrón Unit of Work.
/// Agrupa todas las operaciones de persistencia en una sola transacción atómica.
/// Los handlers de Application dependen de esta interfaz, nunca del DbContext directamente.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos dentro de la transacción actual.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
