using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.Planilla.Queries.GetAllPlanillas;

public class GetAllPlanillasQueryHandler
    : IRequestHandler<GetAllPlanillasQuery, List<PlanillaResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPlanillasQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<PlanillaResponseDto>> Handle(
        GetAllPlanillasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.HistorialPlanillas
            .Include(p => p.Empleado)
            .AsQueryable();

        if (request.IdEmpleado.HasValue)
            query = query.Where(p => p.IdEmpleado == request.IdEmpleado.Value);

        if (!string.IsNullOrWhiteSpace(request.PeriodoMesAnio))
            query = query.Where(p => p.PeriodoMesAnio == request.PeriodoMesAnio);

        return await query
            .OrderByDescending(p => p.PeriodoMesAnio)
            .ThenBy(p => p.Empleado!.Apellidos)
            .Select(p => new PlanillaResponseDto
            {
                IdPlanilla             = p.IdPlanilla,
                IdEmpleado             = p.IdEmpleado,
                NombreEmpleado         = p.Empleado!.Nombres + " " + p.Empleado.Apellidos,
                CargoEmpleado          = p.Empleado.CargoFuncion,
                PeriodoMesAnio         = p.PeriodoMesAnio,
                SalarioBase            = p.SalarioBase,
                Comisiones             = 0m,
                TotalHorasExtras       = p.TotalHorasExtras,
                PagoHorasExtras        = p.PagoHorasExtras,
                Incentivos             = 0m,
                TotalIngresos          = p.SalarioBruto,
                InssLaboral            = p.InssLaboral,
                IrLaboral              = p.IrLaboral,
                Embargo                = 0m,
                Sindicato              = 0m,
                OtrasDeducciones       = p.OtrasDeducciones,
                TotalDeducciones       = p.TotalDeducciones,
                SalarioNeto            = p.SalarioNeto,
                InssPatronal           = Math.Round(p.SalarioBruto * 0.215m, 2),
                Inatec                 = Math.Round(p.SalarioBruto * 0.02m,  2),
                AcumuladoVacaciones    = Math.Round(p.SalarioBase / 30m * 2.5m, 2),
                AcumuladoAguinaldo     = p.AcumuladoAguinaldo,
                AcumuladoIndemnizacion = Math.Round(p.SalarioBase / 30m * 2.5m, 2),
                FechaEmision           = p.FechaEmision
            })
            .ToListAsync(cancellationToken);
    }
}
