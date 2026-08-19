using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.CrearTablaIr;

public class CrearTablaIrCommandHandler : IRequestHandler<CrearTablaIrCommand, TablaIrDto>
{
    private readonly IApplicationDbContext _context;

    public CrearTablaIrCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<TablaIrDto> Handle(CrearTablaIrCommand request, CancellationToken cancellationToken)
    {
        if (request.DesdeMontoAnual < 0)
            throw new InvalidOperationException("El monto inicial del tramo no puede ser negativo.");

        if (request.HastaMontoAnual.HasValue && request.HastaMontoAnual.Value <= request.DesdeMontoAnual)
            throw new InvalidOperationException("El monto final del tramo debe ser mayor al monto inicial.");

        if (request.PorcentajeAplicable < 0 || request.PorcentajeAplicable > 1)
            throw new InvalidOperationException("El porcentaje aplicable debe estar entre 0.0 y 1.0 (ej. 0.15 para 15%).");

        var tramo = new TablaImpuestoRenta
        {
            DesdeMontoAnual     = request.DesdeMontoAnual,
            HastaMontoAnual     = request.HastaMontoAnual,
            PorcentajeAplicable = request.PorcentajeAplicable,
            MontoBaseExceso     = request.MontoBaseExceso,
            CuotaFija           = request.CuotaFija,
            AnioVigencia        = request.AnioVigencia,
            Activo              = request.Activo
        };

        _context.TablaImpuestoRenta.Add(tramo);
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
