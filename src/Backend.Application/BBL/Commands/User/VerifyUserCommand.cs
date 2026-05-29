using System.Net;
using Backend.Application.Common.Results;
using Backend.Domain.Common;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Commands;
public class VerifyUserCommand
{
    public class Command : IRequest<BaseResult>
    {
        public int UserId { get; set; }
        public int Token { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(AppDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Token <= 0 || request.UserId <= 0)
            {
                _logger.LogWarning("Invalid token value: {Token} for user {UserId}", request.Token, request.UserId);

                return new BaseResult(HttpStatusCode.BadRequest, "Invalid token.");
            }

            var record = await _context.VerificationTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == request.UserId &&
                    x.Token == request.Token,
                    cancellationToken);

            if (record == null)
            {
                _logger.LogWarning("Invalid verification attempt for user {UserId}", request.UserId);

                return new BaseResult(HttpStatusCode.BadRequest, "Invalid token.");
            }

            if (record.IsUsed)
                return new BaseResult(HttpStatusCode.BadRequest, "Token already used.");

            if (record.ExpiresAt < DateTime.UtcNow)
                return new BaseResult(HttpStatusCode.BadRequest, "Token expired.");

            record.IsUsed = true;
            record.UsedAt = DateTime.UtcNow;

            var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

            if (user != null)
            {
                user.EmailConfirmed = true;
                user.Status = AccountStatus.Active;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User verified successfully: {UserId}", request.UserId);

            return new BaseResult(HttpStatusCode.OK, "User verified successfully.");
        }
    }
}