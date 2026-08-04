using MipymeAsistencia.Application.Common.Interfaces;

namespace MipymeAsistencia.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Implementación del patrón Unit of Work.
/// Envuelve el ApplicationDbContext y delega el commit a SaveChangesAsync,
/// garantizando que todos los cambios del handler se persistan de forma atómica.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
