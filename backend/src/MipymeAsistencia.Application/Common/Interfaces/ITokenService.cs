namespace MipymeAsistencia.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(string email, string role, int idUsuario, int? idEmpleado);
    string GenerateRefreshToken();
}