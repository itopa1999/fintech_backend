using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Application.DTOs.Auth;
using Backend.Application.Interfaces;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Backend.Application.Commands;

public class LoginUserCommand
{
    public class Command : IRequest<BaseResult<AuthResponseDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<AuthResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<Handler> _logger;
        private readonly AppDbContext _context;
        private readonly ITokenService _token;

        public Handler(
            UserManager<User> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<Handler> logger,
            AppDbContext appContext,
            ITokenService token)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _context = appContext;
            _token = token;
        }

        public async Task<BaseResult<AuthResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt started for email: {Email}", request.Email);

            cancellationToken.ThrowIfCancellationRequested();


            if (!EmailHelper.IsValid(request.Email))
            {
                _logger.LogWarning("Invalid email format attempted: {Email}", request.Email);

                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email format."
                );
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);

                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(
                user,
                request.Password
            );

            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed - invalid password: {Email}", request.Email);

                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            if (user.Status is AccountStatus.Suspended
                or AccountStatus.Closed
                or AccountStatus.Locked)
            {
                _logger.LogWarning("Login blocked due to account status: {Email}, Status: {Status}",
                    request.Email, user.Status);

                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Something is wrong with your account. Please contact support."
                );
            }

            if (!user.EmailConfirmed || user.Status == AccountStatus.Pending)
            {
                _logger.LogWarning("Login blocked - email not confirmed: {Email}", request.Email);

                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please confirm your email before logging in."
                );
            }

            user.LastLoginAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update last login for user {Email}: {Errors}",
                    request.Email,
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            var token = await _jwtTokenGenerator.GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var refreshToken = _token.GenerateRefreshToken();
            var hashedRefreshToken = _token.HashToken(refreshToken);
            var entity = new RefreshToken
            {
                Token = hashedRefreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Token = token,
                RefreshToken = refreshToken,
                Role = roles.FirstOrDefault() ?? string.Empty,
                KycTier = user.KycTier,
                Status = user.Status.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            };

            _logger.LogInformation("Login successful for user: {Email}", request.Email);

            return new BaseResult<AuthResponseDto>(
                HttpStatusCode.OK,
                "Login successful.",
                response
            );
        }
    }
}