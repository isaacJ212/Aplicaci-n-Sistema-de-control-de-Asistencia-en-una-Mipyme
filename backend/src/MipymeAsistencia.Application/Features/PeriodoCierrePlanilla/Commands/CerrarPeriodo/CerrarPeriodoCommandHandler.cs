using MediatR;
using MipymeAsistencia.Application.Common.DTOs.PeriodoCierrePlanilla;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PeriodoCierreEntity = MipymeAsistencia.Domain.Entities.PeriodoCierrePlanilla;

namespace MipymeAsistencia.Application.Features.PeriodoCierrePlanilla.Commands.CerrarPeriodo;

public class CerrarPeriodoCommandHandler : IRequestHandler<CerrarPeriodoCommand, PeriodoCierreDto>
{
    private readonly IApplicationDbContext _context;

    public CerrarPeriodoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<PeriodoCierreDto> Handle(CerrarPeriodoCommand request, CancellationToken cancellationToken)
    {
        var periodoNorm = request.Periodo.Trim();

        var periodo = await _context.PeriodosCierrePlanilla
            .Include(p => p.UsuarioCierre)
            .FirstOrDefaultAsync(p => p.Periodo == periodoNorm, cancellationToken);

        if (periodo is null)
        {
            // Si no existía, creamos el registro de periodo cerrado automáticamente
            var partes = periodoNorm.Split('-');
            var anio = int.Parse(partes[0]);
            var mes  = int.Parse(partes[1]);
            var finMes = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1);

            periodo = new PeriodoCierreEntity
            {
                Periodo               = periodoNorm,
                FechaCorteHorasExtras = finMes,
                FechaEmisionPlanilla  = DateTime.UtcNow,
                Cerrado               = true,
                FechaCierreDefinitivo = DateTime.UtcNow,
                IdUsuarioCierre       = request.IdUsuarioCierre,
                Observaciones         = request.Observaciones?.Trim() ?? "Cierre de periodo ejecutado por Admin"
            };

            _context.PeriodosCierrePlanilla.Add(periodo);
        }
        else
        {
            if (periodo.Cerrado)
                throw new InvalidOperationException($"El periodo '{periodoNorm}' ya se encuentra cerrado.");

            periodo.Cerrado               = true;
            periodo.FechaCierreDefinitivo = DateTime.UtcNow;
            periodo.IdUsuarioCierre       = request.IdUsuarioCierre;
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
                periodo.Observaciones = request.Observaciones.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PeriodoCierreDto
        {
            IdPeriodoCierre       = periodo.IdPeriodoCierre,
            Periodo               = periodo.Periodo,
            FechaCorteHorasExtras = periodo.FechaCorteHorasExtras,
            FechaEmisionPlanilla  = periodo.FechaEmisionPlanilla,
            Cerrado               = periodo.Cerrado,
            FechaCierreDefinitivo = periodo.FechaCierreDefinitivo,
            IdUsuarioCierre       = periodo.IdUsuarioCierre,
            EmailUsuarioCierre    = periodo.UsuarioCierre?.Email,
            Observaciones         = periodo.Observaciones
        };
    }
}
