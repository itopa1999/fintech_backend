using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Application.Interfaces;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.Auth;

public class ResendTokenCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string Email { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public Handler(IEmailService emailService, AppDbContext context, UserManager<User> userManager)
        {
            _emailService = emailService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (user == null)
                return new BaseResult(HttpStatusCode.NotFound, "User not found.");

            if (user.EmailConfirmed)
                return new BaseResult(HttpStatusCode.BadRequest, "Email already verified.");

            var verificationToken = await _context.VerificationTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.TokenType == VerificationTokenType.EmailVerification, cancellationToken)
                ?? new VerificationToken { UserId = user.Id, TokenType = VerificationTokenType.EmailVerification };

            verificationToken.Token = Generators.Generate(6).ToString();
            verificationToken.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
            verificationToken.IsUsed = false;
            verificationToken.UsedAt = null;
            verificationToken.CreatedAt = verificationToken.CreatedAt == default ? DateTime.UtcNow : verificationToken.CreatedAt;

            if (verificationToken.Id == 0)
                _context.VerificationTokens.Add(verificationToken);
            else
                _context.VerificationTokens.Update(verificationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await _emailService.SendVerificationOtpAsync(user.FirstName ?? string.Empty, user.Email, verificationToken.Token, cancellationToken);

            return new BaseResult(HttpStatusCode.OK, $"A token has been resent to {request.Email} if an account with that email exists.");
        }
    }
}