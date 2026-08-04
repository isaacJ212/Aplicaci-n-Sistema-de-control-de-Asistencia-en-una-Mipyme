using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Sede;

namespace MipymeAsistencia.Application.Features.Sede.Queries.GetSede;

/// <summary>
/// Query CQRS que retorna la configuración actual de la única sede del sistema.
/// </summary>
public class GetSedeQuery : IRequest<SedeResponseDto> { }
