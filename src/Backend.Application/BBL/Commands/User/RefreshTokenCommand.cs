using System.Net;
using Backend.Application.Common.Results;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.User;

public class RefreshTokenCommand
{
    public class Command : IRequest<BaseResult<RefreshRespondDto>>
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshRespondDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<RefreshRespondDto>>
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IJwtTokenGenerator _jwt;

        public Handler(
            AppDbContext context,
            ITokenService tokenService,
            IJwtTokenGenerator jwt)
        {
            _context = context;
            _tokenService = tokenService;
            _jwt = jwt;
        }

        public async Task<BaseResult<RefreshRespondDto>> Handle(Command request,CancellationToken cancellationToken)
        {
            var hash = _tokenService.HashToken(request.RefreshToken);
            var stored = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == hash, cancellationToken);

            if (stored == null || !stored.IsActive)
            {
                return new BaseResult<RefreshRespondDto>(
                    HttpStatusCode.Unauthorized,
                    "Invalid or expired refresh token."
                );
            }

            stored.RevokedAt = DateTime.UtcNow;
            var newRefresh = _tokenService.GenerateRefreshToken();
            var newHash = _tokenService.HashToken(newRefresh);
            var newJwt = await _jwt.GenerateToken(stored.User);

            var newEntity = new RefreshToken
            {
                UserId = stored.UserId,
                Token = newHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(newEntity);
            await _context.SaveChangesAsync(cancellationToken);

            var response =  new RefreshRespondDto
            {
                Token = newJwt,
                RefreshToken = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            };
            return new BaseResult<RefreshRespondDto>(HttpStatusCode.OK, "Token refreshed successfully.", response);
        }
    }

}