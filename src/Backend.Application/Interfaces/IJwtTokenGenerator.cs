using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}