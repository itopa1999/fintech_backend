using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateToken(User user);
}