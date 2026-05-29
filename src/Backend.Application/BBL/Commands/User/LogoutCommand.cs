using System.Net;
using Backend.Application.Common.Results;
using Backend.Application.Interfaces;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.User;

public class LogoutCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string RefreshToken { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public Handler(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var hash = _tokenService.HashToken(request.RefreshToken);
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == hash, cancellationToken);

            if (token == null)
            {
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "Invalid refresh token."
                );
            }

            if (token.RevokedAt != null)
            {
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "Refresh token has already been revoked."
                );
            }

            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new BaseResult(
                HttpStatusCode.OK,
                "Logged out successfully."
            );
        }
    }
}