using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Commands.CrearDiaFeriado;

public class CrearDiaFeriadoCommandHandler : IRequestHandler<CrearDiaFeriadoCommand, DiaFeriadoDto>
{
    private readonly IApplicationDbContext _context;

    public CrearDiaFeriadoCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<DiaFeriadoDto> Handle(CrearDiaFeriadoCommand request, CancellationToken cancellationToken)
    {
        var fechaNormalizada = request.Fecha.Date.ToUniversalTime();

        var existe = await _context.DiasFeriados
            .AnyAsync(f => f.Fecha.Date == fechaNormalizada.Date, cancellationToken);

        if (existe)
            throw new InvalidOperationException($"Ya existe un día feriado registrado para la fecha {fechaNormalizada:yyyy-MM-dd}.");

        var feriado = new DiaFeriado
        {
            Fecha         = fechaNormalizada,
            Nombre        = request.Nombre.Trim(),
            Descripcion   = request.Descripcion?.Trim(),
            EsRecuperable = request.EsRecuperable,
            EsMovil       = request.EsMovil
        };

        _context.DiasFeriados.Add(feriado);
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
