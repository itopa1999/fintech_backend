using System.Net;
using Backend.Application.Common.Results;
using Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.BBL.Commands.Auth;

public class ChangePasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
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
            if (request.NewPassword != request.ConfirmPassword)
            return new BaseResult(HttpStatusCode.BadRequest, "Passwords do not match.");

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return new BaseResult(HttpStatusCode.NotFound, "User not found.");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
                return new BaseResult(HttpStatusCode.BadRequest, "Current password is incorrect.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new BaseResult(HttpStatusCode.BadRequest, $"Password reset failed: {errors}");
            }

            return new BaseResult(HttpStatusCode.OK, "Password changed successfully.");
        }
    }
}