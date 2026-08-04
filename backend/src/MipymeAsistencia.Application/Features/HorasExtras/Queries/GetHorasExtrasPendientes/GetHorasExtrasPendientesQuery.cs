using MediatR;
using MipymeAsistencia.Application.Common.DTOs.HoraExtra;

namespace MipymeAsistencia.Application.Features.HorasExtras.Queries.GetHorasExtrasPendientes;

/// <summary>
/// Retorna todas las horas extras en estado "Pendiente" de todos los empleados.
/// Solo accesible por Admin.
/// </summary>
public class GetHorasExtrasPendientesQuery : IRequest<List<HoraExtraResponseDto>> { }
