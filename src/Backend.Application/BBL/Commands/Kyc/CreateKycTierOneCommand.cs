using System.Net;
using Backend.Application.Common.Helpers;
using Backend.Application.Common.Results;
using Backend.Domain.Common;
using Backend.Domain.Entities;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.Kyc;

public class CreateKycTierOneCommand
{
    public class Command : IRequest<BaseResult<CreateKycTierOneResponse>>
    {
        public int UserId { get; set; }
        public string BVN { get; set; }
        public string NIN { get; set; }
    }

    public class CreateKycTierOneResponse
    {
        public string BVN { get; set; }
        public string NIN { get; set; }
        public string VerificationStatus { get; set; }
        public string Message { get; set; }

    }

    public class Handler : IRequestHandler<Command, BaseResult<CreateKycTierOneResponse>>
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public Handler(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<BaseResult<CreateKycTierOneResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var existingKyc = await _context.KycProfiles.FirstOrDefaultAsync(k => k.UserId == request.UserId, cancellationToken);
            if (existingKyc != null && existingKyc.IsTierOneComplete())
                return new BaseResult<CreateKycTierOneResponse>(HttpStatusCode.BadRequest, "KYC information cannot be changed", new CreateKycTierOneResponse { Message = "KYC information cannot be changed" });
            
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new BaseResult<CreateKycTierOneResponse>(HttpStatusCode.NotFound, "User not found", new CreateKycTierOneResponse { Message = "User not found" });

            var kyc = new KycProfile
            {
                UserId = request.UserId,
                BVN = request.BVN,
                NIN = request.NIN,
                VerificationStatus = VerificationStatus.Pending,
            };

            await _context.KycProfiles.AddAsync(kyc, cancellationToken);
            
            user.KycTier = KycTier.Tier1;

            await _context.SaveChangesAsync(cancellationToken);

            var kycResponse = new CreateKycTierOneResponse
            {
                BVN = kyc.BVN,
                NIN = kyc.NIN,
                VerificationStatus = kyc.VerificationStatus.ToString(),
                Message = kyc.VerificationStatus switch
                {
                    VerificationStatus.Pending => "KYC information submitted successfully. Verification is pending.",
                    VerificationStatus.Approved => "KYC information submitted successfully. Your KYC has been approved.",
                    VerificationStatus.Rejected => "KYC information submitted successfully. Unfortunately, your KYC has been rejected.",
                    _ => "KYC information submitted successfully."
                }
            };

            return new BaseResult<CreateKycTierOneResponse>(HttpStatusCode.OK, "KYC information submitted successfully.", kycResponse);
        }
    }
}