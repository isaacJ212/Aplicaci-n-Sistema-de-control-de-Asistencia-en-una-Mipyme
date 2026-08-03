using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Auth;

namespace MipymeAsistencia.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query CQRS para obtener los datos del usuario autenticado.
/// El email se extrae del claim del JWT en el controlador.
/// </summary>
public class GetCurrentUserQuery : IRequest<CurrentUserDto>
{
    public string Email { get; set; } = string.Empty;
}
