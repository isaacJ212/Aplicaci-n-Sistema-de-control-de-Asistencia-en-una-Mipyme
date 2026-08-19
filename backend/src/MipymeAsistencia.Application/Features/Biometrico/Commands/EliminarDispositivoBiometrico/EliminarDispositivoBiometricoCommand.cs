using MediatR;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Biometrico.Commands.EliminarDispositivoBiometrico;

public class EliminarDispositivoBiometricoCommand : IRequest<bool>
{
    public int IdDispositivo { get; set; }
}

public class EliminarDispositivoBiometricoCommandHandler : IRequestHandler<EliminarDispositivoBiometricoCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public EliminarDispositivoBiometricoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<bool> Handle(EliminarDispositivoBiometricoCommand request, CancellationToken cancellationToken)
    {
        var d = await _context.DispositivosBiometricos
            .FirstOrDefaultAsync(x => x.IdDispositivo == request.IdDispositivo, cancellationToken);

        if (d is null)
            throw new KeyNotFoundException($"Dispositivo biométrico #{request.IdDispositivo} no encontrado.");

        _context.DispositivosBiometricos.Remove(d);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
