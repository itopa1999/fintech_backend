using System.Net;
using Backend.Application.Common.Results;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.Auth;

public class ConfirmForgotPasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        public Handler(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return new BaseResult(HttpStatusCode.BadRequest, "Token is required.");
            }
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new BaseResult(HttpStatusCode.BadRequest, "Passwords do not match.");
            }
            var verificationToken = await _context.VerificationTokens
                .FirstOrDefaultAsync(t => t.Token == request.Token && t.TokenType == VerificationTokenType.PasswordReset, cancellationToken);
            
            if (verificationToken == null || verificationToken.IsUsed || verificationToken.ExpiresAt < DateTime.UtcNow)
            {
                return new BaseResult(HttpStatusCode.BadRequest, "Invalid or expired token.");
            }

            var user = await _userManager.FindByIdAsync(verificationToken.UserId.ToString());
            if (user == null)
            {
                return new BaseResult(HttpStatusCode.BadRequest, "User not found.");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new BaseResult(HttpStatusCode.BadRequest, $"Password reset failed: {errors}");
            }

            verificationToken.IsUsed = true;
            verificationToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new BaseResult(HttpStatusCode.OK, "Password has been reset successfully.");
        }
    }

}