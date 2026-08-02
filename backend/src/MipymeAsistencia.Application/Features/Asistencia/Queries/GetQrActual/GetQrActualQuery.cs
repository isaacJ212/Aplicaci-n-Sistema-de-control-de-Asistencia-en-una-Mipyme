using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetQrActual;

public class GetQrActualQuery : IRequest<QrActualResponseDto> { }
