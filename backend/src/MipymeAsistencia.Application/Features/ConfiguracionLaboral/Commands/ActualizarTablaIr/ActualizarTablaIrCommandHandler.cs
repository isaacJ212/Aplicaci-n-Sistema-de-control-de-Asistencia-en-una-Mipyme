using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.ActualizarTablaIr;

public class ActualizarTablaIrCommandHandler : IRequestHandler<ActualizarTablaIrCommand, TablaIrDto>
{
    private readonly IApplicationDbContext _context;

    public ActualizarTablaIrCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<TablaIrDto> Handle(ActualizarTablaIrCommand request, CancellationToken cancellationToken)
    {
        var tramo = await _context.TablaImpuestoRenta
            .FirstOrDefaultAsync(t => t.IdTablaIr == request.IdTablaIr, cancellationToken);

        if (tramo is null)
            throw new KeyNotFoundException($"Tramo de tabla IR con id {request.IdTablaIr} no encontrado.");

        if (request.DesdeMontoAnual < 0)
            throw new InvalidOperationException("El monto inicial del tramo no puede ser negativo.");

        if (request.HastaMontoAnual.HasValue && request.HastaMontoAnual.Value <= request.DesdeMontoAnual)
            throw new InvalidOperationException("El monto final del tramo debe ser mayor al monto inicial.");

        if (request.PorcentajeAplicable < 0 || request.PorcentajeAplicable > 1)
            throw new InvalidOperationException("El porcentaje aplicable debe estar entre 0.0 y 1.0 (ej. 0.15 para 15%).");

        tramo.DesdeMontoAnual     = request.DesdeMontoAnual;
        tramo.HastaMontoAnual     = request.HastaMontoAnual;
        tramo.PorcentajeAplicable = request.PorcentajeAplicable;
        tramo.MontoBaseExceso     = request.MontoBaseExceso;
        tramo.CuotaFija           = request.CuotaFija;
        tramo.AnioVigencia        = request.AnioVigencia;
        tramo.Activo              = request.Activo;

        await _context.SaveChangesAsync(cancellationToken);

        return new TablaIrDto
        {
            IdTablaIr           = tramo.IdTablaIr,
            DesdeMontoAnual     = tramo.DesdeMontoAnual,
            HastaMontoAnual     = tramo.HastaMontoAnual,
            PorcentajeAplicable = tramo.PorcentajeAplicable,
            MontoBaseExceso     = tramo.MontoBaseExceso,
            CuotaFija           = tramo.CuotaFija,
            AnioVigencia        = tramo.AnioVigencia,
            Activo              = tramo.Activo
        };
    }
}
