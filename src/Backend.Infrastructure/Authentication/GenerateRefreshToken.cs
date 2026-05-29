using System.Security.Cryptography;
using System.Text;
using Backend.Application.Interfaces;

namespace Backend.Infrastructure.Authentication;

public class TokenService : ITokenService
{
    public string GenerateRefreshToken()
    {
        var bytes = new byte[599];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}