namespace Backend.Application.Interfaces;

public interface ITokenService
{
    string GenerateRefreshToken();
    string HashToken(string token);
}