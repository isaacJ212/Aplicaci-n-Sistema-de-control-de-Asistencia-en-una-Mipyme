namespace MipymeAsistencia.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(string email, string role);
    string GenerateRefreshToken();
}