using System.Net;
using Backend.Application.Common.Results;
using Backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.BBL.Commands.Auth;

public class ForgotPasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string Email { get; set; }
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
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new BaseResult(HttpStatusCode.OK, "If an account with that email exists, a password reset link has been sent.");
            }

            return new BaseResult(HttpStatusCode.OK, "If an account with that email exists, a password reset link has been sent.");
        }
    }
}