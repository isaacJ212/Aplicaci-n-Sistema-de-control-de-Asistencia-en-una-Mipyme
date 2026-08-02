using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Empleado;

namespace MipymeAsistencia.Application.Features.Empleado.Queries.GetAllEmpleados;

public class GetAllEmpleadosQuery : IRequest<List<EmpleadoResponseDto>> { }
