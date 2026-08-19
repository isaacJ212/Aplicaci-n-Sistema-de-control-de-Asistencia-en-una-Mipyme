using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetInformeAsistencia;

/// <summary>
/// Genera un informe de asistencia por empleado para el período indicado.
/// Si IdEmpleado es null devuelve el informe de todos los empleados.
/// </summary>
public class GetInformeAsistenciaQuery : IRequest<List<InformeAsistenciaDto>>
{
    public int?      IdEmpleado  { get; set; }
    public DateTime  FechaDesde  { get; set; }
    public DateTime  FechaHasta  { get; set; }
}
