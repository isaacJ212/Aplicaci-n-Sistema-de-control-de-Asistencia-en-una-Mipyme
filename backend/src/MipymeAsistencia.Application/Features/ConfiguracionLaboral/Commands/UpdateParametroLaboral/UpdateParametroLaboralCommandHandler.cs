using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.UpdateParametroLaboral;

public class UpdateParametroLaboralCommandHandler : IRequestHandler<UpdateParametroLaboralCommand, ParametroLaboralDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateParametroLaboralCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<ParametroLaboralDto> Handle(UpdateParametroLaboralCommand request, CancellationToken cancellationToken)
    {
        var claveNormalizada = request.Clave.Trim().ToUpperInvariant();

        var parametro = await _context.ParametrosLaborales
            .FirstOrDefaultAsync(p => p.Clave == claveNormalizada, cancellationToken);

        if (parametro is null)
            throw new KeyNotFoundException($"Parámetro laboral '{request.Clave}' no encontrado.");

        if (request.Valor < 0)
            throw new InvalidOperationException("El valor del parámetro no puede ser negativo.");

        parametro.Valor = request.Valor;
        if (!string.IsNullOrWhiteSpace(request.Descripcion))
        {
            parametro.Descripcion = request.Descripcion.Trim();
        }
        parametro.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ParametroLaboralDto
        {
            IdParametro       = parametro.IdParametro,
            Clave             = parametro.Clave,
            Valor             = parametro.Valor,
            Descripcion       = parametro.Descripcion,
            FechaModificacion = parametro.FechaModificacion
        };
    }
}
