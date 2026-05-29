using System.Net;
using System.Security.Cryptography;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Commands;

public class RegisterUserCommand
{
    public class Command : IRequest<BaseResult<ResponseDto>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RoleType { get; set; }
    }

    public class ResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<ResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(
            UserManager<User> userManager,
            AppDbContext context,
            ILogger<Handler> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResult<ResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RegisterUser started for Email: {Email}", request.Email);

            if (!EmailHelper.IsValid(request.Email))
            {
                _logger.LogWarning("Invalid email format: {Email}", request.Email);

                return new BaseResult<ResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email format."
                );
            }

            if (!Enum.TryParse<UserRole>(request.RoleType, true, out var role))
            {
                _logger.LogWarning("Invalid role type: {RoleType}", request.RoleType);

                return new BaseResult<ResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid role type."
                );
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User already exists: {Email}", request.Email);

                return new BaseResult<ResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Email is already registered."
                );
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                Status = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();

                _logger.LogError("User creation failed for {Email}. Errors: {Errors}",
                    request.Email,
                    string.Join(", ", errors));

                return new BaseResult<ResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User creation failed: " + string.Join(", ", errors)
                );
            }

            _logger.LogInformation("User created successfully: {Email}, Id: {UserId}",
                request.Email,
                user.Id);

            var roleName = role.ToString();

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Role assignment failed for {Email}", request.Email);
            }
            else
            {
                _logger.LogInformation("Role {Role} assigned to user {Email}", roleName, request.Email);
            }

            var token = RandomNumberGenerator.GetInt32(100000, 1000000);

            _logger.LogInformation("Verification token generated for user {Email}", request.Email);

            Console.WriteLine($"VERIFY TOKEN: {token}");

            var verificationToken = new VerificationToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.VerificationTokens.Add(verificationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Verification token saved for user {Email}", request.Email);

            var response = new ResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roleName,
            };

            return new BaseResult<ResponseDto>(
                HttpStatusCode.OK,
                "User created successfully. Verification token generated.",
                response
            );
        }
    }
}