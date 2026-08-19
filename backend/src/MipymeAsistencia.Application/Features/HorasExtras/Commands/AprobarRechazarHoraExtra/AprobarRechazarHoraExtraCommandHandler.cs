using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.HorasExtras.Commands.AprobarRechazarHoraExtra;

public class AprobarRechazarHoraExtraCommandHandler
    : IRequestHandler<AprobarRechazarHoraExtraCommand, HoraExtraResponseDto>
{
    private readonly IApplicationDbContext _context;

    public AprobarRechazarHoraExtraCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<HoraExtraResponseDto> Handle(
        AprobarRechazarHoraExtraCommand request, CancellationToken cancellationToken)
    {
        var horaExtra = await _context.HorasExtras
            .Include(h => h.Empleado)
            .Include(h => h.UsuarioAprobador)
            .FirstOrDefaultAsync(h => h.IdHoraExtra == request.IdHoraExtra, cancellationToken);

        if (horaExtra is null)
            throw new KeyNotFoundException($"Hora extra con id {request.IdHoraExtra} no encontrada.");

        if (horaExtra.Estado != "Pendiente")
            throw new InvalidOperationException(
                $"La hora extra ya fue {horaExtra.Estado.ToLower()}. Solo se pueden gestionar registros en estado Pendiente.");

        var periodoStr = horaExtra.Fecha.ToString("yyyy-MM");
        var periodoCierre = await _context.PeriodosCierrePlanilla
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Periodo == periodoStr, cancellationToken);

        if (periodoCierre != null && periodoCierre.Cerrado)
            throw new InvalidOperationException(
                $"No se pueden aprobar ni rechazar horas extras del periodo {periodoStr} porque ya se encuentra cerrado.");

        horaExtra.Estado             = request.Estado;
        horaExtra.IdUsuarioAprobador = request.IdUsuarioAprobador;

        await _context.SaveChangesAsync(cancellationToken);

        return new HoraExtraResponseDto
        {
            IdHoraExtra        = horaExtra.IdHoraExtra,
            IdEmpleado         = horaExtra.IdEmpleado,
            NombreEmpleado     = horaExtra.Empleado!.Nombres + " " + horaExtra.Empleado.Apellidos,
            IdUsuarioAprobador = horaExtra.IdUsuarioAprobador,
            NombreAprobador    = horaExtra.UsuarioAprobador?.Email,
            Fecha              = horaExtra.Fecha,
            CantidadHoras      = horaExtra.CantidadHoras,
            Motivo             = horaExtra.Motivo,
            MontoPagar         = horaExtra.MontoPagar,
            Estado             = horaExtra.Estado
        };
    }
}
