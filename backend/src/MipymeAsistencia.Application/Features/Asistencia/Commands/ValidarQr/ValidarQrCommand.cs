using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Commands.ValidarQr;

public class ValidarQrCommand : IRequest<ValidarQrResponseDto>
{
    public int IdEmpleado { get; set; }
    public string TokenQrEscaneado { get; set; } = string.Empty;
}
