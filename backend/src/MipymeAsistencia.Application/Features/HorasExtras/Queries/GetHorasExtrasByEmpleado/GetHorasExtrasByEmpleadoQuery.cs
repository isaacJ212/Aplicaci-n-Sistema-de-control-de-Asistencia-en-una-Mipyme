using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;

namespace MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasByEmpleado;

/// <summary>
/// Retorna todas las horas extras de un empleado específico,
/// ordenadas de más reciente a más antigua.
/// </summary>
public class GetHorasExtrasByEmpleadoQuery : IRequest<List<HoraExtraResponseDto>>
{
    public int IdEmpleado { get; set; }
}
