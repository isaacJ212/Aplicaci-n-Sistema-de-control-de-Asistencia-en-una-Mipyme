using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Commands.ActualizarDiaFeriado;

public class ActualizarDiaFeriadoCommandHandler : IRequestHandler<ActualizarDiaFeriadoCommand, DiaFeriadoDto>
{
    private readonly IApplicationDbContext _context;

    public ActualizarDiaFeriadoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DiaFeriadoDto> Handle(ActualizarDiaFeriadoCommand request, CancellationToken cancellationToken)
    {
        var feriado = await _context.DiasFeriados
            .FirstOrDefaultAsync(f => f.IdDiaFeriado == request.IdDiaFeriado, cancellationToken);

        if (feriado is null)
            throw new KeyNotFoundException($"Día feriado con id {request.IdDiaFeriado} no encontrado.");

        var fechaNormalizada = request.Fecha.Date.ToUniversalTime();

        var existeOtro = await _context.DiasFeriados
            .AnyAsync(f => f.IdDiaFeriado != request.IdDiaFeriado && f.Fecha.Date == fechaNormalizada.Date, cancellationToken);

        if (existeOtro)
            throw new InvalidOperationException($"Ya existe otro día feriado registrado para la fecha {fechaNormalizada:yyyy-MM-dd}.");

        feriado.Fecha         = fechaNormalizada;
        feriado.Nombre        = request.Nombre.Trim();
        feriado.Descripcion   = request.Descripcion?.Trim();
        feriado.EsRecuperable = request.EsRecuperable;
        feriado.EsMovil       = request.EsMovil;

        await _context.SaveChangesAsync(cancellationToken);

        return new DiaFeriadoDto
        {
            IdDiaFeriado = feriado.IdDiaFeriado,
            Fecha        = feriado.Fecha,
            FechaFormato = feriado.Fecha.ToString("yyyy-MM-dd"),
            Nombre       = feriado.Nombre,
            Descripcion  = feriado.Descripcion,
            EsRecuperable = feriado.EsRecuperable,
            EsMovil      = feriado.EsMovil
        };
    }
}
