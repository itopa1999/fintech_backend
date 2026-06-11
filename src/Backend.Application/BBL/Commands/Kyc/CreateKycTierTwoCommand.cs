using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Application.Interfaces;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Application.BBL.Commands.Kyc;

public class CreateKycTierTwoCommand
{
    public class Command : IRequest<BaseResult<CreateKycTierTwoResponse>>
    {
        public int UserId { get; set; }
        public IFormFile IdDocument { get; set; }
        public IFormFile Selfie { get; set; }
    }

    public class CreateKycTierTwoResponse
    {
        public string IdDocumentUrl { get; set; }
        public string SelfieUrl { get; set; }
        public string VerificationStatus { get; set; }
        public string Message { get; set; }

    }

    public class Handler : IRequestHandler<Command, BaseResult<CreateKycTierTwoResponse>>
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<Handler> _logger;
        public Handler(AppDbContext context,
            UserManager<User> userManager, 
            IFileStorageService fileStorage, 
            ILogger<Handler> logger
        )
        {
            _context = context;
            _userManager = userManager;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<BaseResult<CreateKycTierTwoResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            // 1. Validate files
            if (request.IdDocument == null || request.IdDocument.Length == 0)
                return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.BadRequest, "IdDocument is required");
            if (request.Selfie == null || request.Selfie.Length == 0)
                return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.BadRequest, "Selfie is required");


            var existingKyc = await _context.KycProfiles.FirstOrDefaultAsync(k => k.UserId == request.UserId, cancellationToken);
            if (existingKyc == null || 
                !existingKyc.IsTierOneComplete() || 
                existingKyc.VerificationStatus != VerificationStatus.Approved)
            {
                return new BaseResult<CreateKycTierTwoResponse>(
                    HttpStatusCode.BadRequest, 
                    "Tier 1 KYC is not complete or not approved.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.BadRequest, "User not found");

            // 3. Validate file types (e.g., images only)
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            if (!allowedTypes.Contains(request.IdDocument.ContentType) || !allowedTypes.Contains(request.Selfie.ContentType))
                return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.BadRequest, "Only JPEG/PNG images are allowed");

            // 4. Upload files
            string idDocUrl, selfieUrl;
            try
            {
                idDocUrl = await _fileStorage.UploadFileAsync(request.IdDocument, $"kyc/{request.UserId}/documents", cancellationToken);
                selfieUrl = await _fileStorage.UploadFileAsync(request.Selfie, $"kyc/{request.UserId}/selfies", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed for user {UserId}", request.UserId);
                return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.InternalServerError, "File upload failed. Please try again.");
            }

            // 5. Update or create KYC Tier Two record (assuming separate table or update existing profile)
            var kyc = await _context.KycProfiles.FirstOrDefaultAsync(k => k.UserId == request.UserId, cancellationToken);
            if (kyc == null)
            {
                kyc = new KycProfile { UserId = request.UserId };
                _context.KycProfiles.Add(kyc);
            }

            kyc.IdDocumentUrl = idDocUrl;
            kyc.SelfieUrl = selfieUrl;
            kyc.VerificationStatus = VerificationStatus.Pending; // Tier two requires manual verification
            kyc.KycTier = (int)KycTier.Tier2; // Assuming you have this field

            await _context.SaveChangesAsync(cancellationToken);

            // 6. Prepare response
            var response = new CreateKycTierTwoResponse
            {
                IdDocumentUrl = idDocUrl,
                SelfieUrl = selfieUrl,
                VerificationStatus = kyc.VerificationStatus.ToString(),
                Message = "KYC Tier Two information submitted successfully. Verification is pending."
            };

            return new BaseResult<CreateKycTierTwoResponse>(HttpStatusCode.OK, "KYC Tier Two submitted successfully.", response);
        }
    }
        }
    }

}