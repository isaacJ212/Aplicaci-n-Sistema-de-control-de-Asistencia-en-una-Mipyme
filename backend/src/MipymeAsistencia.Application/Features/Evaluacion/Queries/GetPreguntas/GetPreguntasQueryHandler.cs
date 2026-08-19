using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;
using MipymeAsistencia.Domain.Services;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetPreguntas;

public class GetPreguntasQueryHandler : IRequestHandler<GetPreguntasQuery, List<PreguntaDto>>
{
    public Task<List<PreguntaDto>> Handle(
        GetPreguntasQuery request, CancellationToken cancellationToken)
    {
        var lista = Evaluacion360Preguntas.Catalogo
            .Select(p => new PreguntaDto
            {
                Numero    = p.Numero,
                Categoria = p.Categoria,
                Texto     = p.Texto,
                Tipo      = p.Tipo,
                Peso      = p.Peso,
            }).ToList();

        return Task.FromResult(lista);
    }
}
