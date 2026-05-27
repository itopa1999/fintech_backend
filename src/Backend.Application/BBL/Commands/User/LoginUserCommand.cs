using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Commands;

public class LoginUserCommand
{
    public class Command : IRequest<BaseResult<AuthResponseDto>>
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<AuthResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public Handler(
            UserManager<User> userManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<BaseResult<AuthResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            if (!EmailHelper.IsValid(request.Email))
            {
                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email format."
                );
            }
            
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
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
                return new BaseResult<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            user.LastLoginAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            return new BaseResult<AuthResponseDto>(
                HttpStatusCode.OK,
                "Login successful.",
                response
            );
        }
    }
}