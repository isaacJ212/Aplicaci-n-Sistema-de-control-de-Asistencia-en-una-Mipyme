using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;

namespace MipymeAsistencia.Application.Features.HorasExtras.Commands.RegistrarHoraExtra;

public class RegistrarHoraExtraCommand : IRequest<HoraExtraResponseDto>
{
    public int      IdEmpleado    { get; set; }
    public DateTime Fecha         { get; set; }
    public decimal  CantidadHoras { get; set; }
    public string   Motivo        { get; set; } = string.Empty;
    public decimal  FactorRecargo { get; set; } = 1.5m;
}
