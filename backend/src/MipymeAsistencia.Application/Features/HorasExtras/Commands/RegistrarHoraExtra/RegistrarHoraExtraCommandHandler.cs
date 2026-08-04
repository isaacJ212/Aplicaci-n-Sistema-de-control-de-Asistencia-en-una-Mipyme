using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.HorasExtras.Commands.RegistrarHoraExtra;

public class RegistrarHoraExtraCommandHandler
    : IRequestHandler<RegistrarHoraExtraCommand, HoraExtraResponseDto>
{
    private readonly IApplicationDbContext _context;

    public RegistrarHoraExtraCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<HoraExtraResponseDto> Handle(
        RegistrarHoraExtraCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken);

        if (empleado is null)
            throw new KeyNotFoundException($"Empleado con id {request.IdEmpleado} no encontrado.");

        // Fórmula Arto. 62 Ley 185 Nicaragua:
        // MontoPagar = (SalarioMensual / 240) * FactorRecargo * CantidadHoras
        // 240 = horas laborales promedio al mes (8 horas * 30 días)
        var montoHora   = empleado.SalarioBaseMensual / 240m;
        var montoPagar  = Math.Round(montoHora * request.FactorRecargo * request.CantidadHoras, 2);

        var horaExtra = new HoraExtra
        {
            IdEmpleado    = request.IdEmpleado,
            Fecha         = request.Fecha.ToUniversalTime(),
            CantidadHoras = request.CantidadHoras,
            Motivo        = request.Motivo,
            MontoPagar    = montoPagar,
            Estado        = "Pendiente"          // Inicia pendiente de aprobación
        };

        _context.HorasExtras.Add(horaExtra);
        await _context.SaveChangesAsync(cancellationToken);

        return new HoraExtraResponseDto
        {
            IdHoraExtra        = horaExtra.IdHoraExtra,
            IdEmpleado         = horaExtra.IdEmpleado,
            NombreEmpleado     = empleado.Nombres + " " + empleado.Apellidos,
            IdUsuarioAprobador = null,
            NombreAprobador    = null,
            Fecha              = horaExtra.Fecha,
            CantidadHoras      = horaExtra.CantidadHoras,
            Motivo             = horaExtra.Motivo,
            MontoPagar         = horaExtra.MontoPagar,
            Estado             = horaExtra.Estado
        };
    }
}
