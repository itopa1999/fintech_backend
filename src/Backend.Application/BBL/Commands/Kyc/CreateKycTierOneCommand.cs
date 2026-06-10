using System.Net;
using Backend.Application.Common.Results;
using Backend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.BBL.Commands.Kyc;

public class CreateKycTierOneCommand
{
    public class Command : IRequest<BaseResult>
    {
        public int UserId { get; set; }
        public string BVN { get; set; }
        public string NIN { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly AppDbContext _context;
        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var exisitingKyc = await _context.KycProfiles.FirstOrDefaultAsync(k => k.UserId == request.UserId, cancellationToken);
            if (exisitingKyc != null && (exisitingKyc.BVN == request.BVN || exisitingKyc.NIN == request.NIN))
                return new BaseResult(HttpStatusCode.BadRequest, "KYC information cannot be changed");
            
            var kyc = new Kyc
            {
                UserId = request.UserId,
                FullName = request.FullName,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                IdNumber = request.IdNumber,
                IdDocumentPath = await SaveIdDocumentAsync(request.IdDocument)
            };

            _context.Kycs.Add(kyc);
            await _context.SaveChangesAsync(cancellationToken);

            return new BaseResult(HttpStatusCode.OK, "KYC information submitted successfully.");
        }

        private async Task<string> SaveIdDocumentAsync(IFormFile idDocument)
        {
            // Implement logic to save the ID document and return its path
            // This is a placeholder implementation
            var filePath = Path.Combine("uploads", Guid.NewGuid().ToString() + Path.GetExtension(idDocument.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await idDocument.CopyToAsync(stream);
            }
            return filePath;
        }
    }
}