using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;

namespace MipymeAsistencia.Application.Features.Empleado.Commands.AcumularVacaciones;

/// <summary>
/// Recalcula y actualiza DiasVacacionesAcumuladas de un empleado.
/// Tasa: 2.5 días por mes trabajado (0.0833 días/día trabajado).
/// Se basa en la FechaContratacion y en el historial de asistencia real.
/// </summary>
public class AcumularVacacionesCommand : IRequest<AcumularVacacionesResponseDto>
{
    public int IdEmpleado { get; set; }
}
