using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Evaluacion;

namespace MipymeAsistencia.Application.Features.Evaluacion.Queries.GetPreguntas;

/// <summary>Devuelve el catálogo completo de 20 preguntas con sus pesos.</summary>
public class GetPreguntasQuery : IRequest<List<PreguntaDto>> { }
