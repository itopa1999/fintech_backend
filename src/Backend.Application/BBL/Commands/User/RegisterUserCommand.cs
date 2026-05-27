using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Commands;

public class RegisterUserCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly UserManager<User> _userManager;

        public Handler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!EmailHelper.IsValid(request.Email))
            {
                return new BaseResult(
                    statusCode: HttpStatusCode.BadRequest,
                    message: "Invalid email format."

                );
            }
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new BaseResult(
                    statusCode: HttpStatusCode.BadRequest,
                    message: "Email is already registered."
                );
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            // send email confirmation here
            

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description)
                    .ToArray();

                return new BaseResult(
                    statusCode: HttpStatusCode.BadRequest,
                    message: "User creation failed: " + string.Join(", ", errors)
                );
            }

            return new BaseResult(
                statusCode: HttpStatusCode.OK,
                message: "User created successfully."
            );
        }
    }
}