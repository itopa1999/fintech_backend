using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Domain.Entities;
using Backend.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Backend.Application.Common.Settings;
using Microsoft.AspNetCore.Identity;

namespace Backend.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> _userManager;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions, UserManager<User> userManager)
    {
        _jwtSettings = jwtOptions.Value;
        _userManager = userManager;
    }

    public async Task<string> GenerateToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),

            new Claim(ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(ClaimTypes.Email,
                user.Email ?? string.Empty),

            new Claim(ClaimTypes.Name,
                user.FullName),

            new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role =>
            new Claim(ClaimTypes.Role, role)
        ));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expires = DateTime.UtcNow.AddMinutes(
            _jwtSettings.DurationInMinutes
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}