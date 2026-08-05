using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MipymeAsistencia.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MipymeAsistencia.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email, string role, int idUsuario, int? idEmpleado)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "SuperSecureJwtSecretKeyForMipymeAsistencia123";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "MipymeAsistencia";
        var audience = _configuration["JwtSettings:Audience"] ?? "MipymeAsistenciaClients";
        var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 120);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("idUsuario", idUsuario.ToString())
        };

        if (idEmpleado.HasValue)
        {
            claims.Add(new Claim("idEmpleado", idEmpleado.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes);
    }
}
