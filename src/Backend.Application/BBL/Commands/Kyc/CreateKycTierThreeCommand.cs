using System.Net;
using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.BBL.Commands.Kyc;

public class CreateKycTierThreeCommand
{
    public class Command : IRequest<BaseResult>
    {
        public int UserId { get; set; }
        public string AddressLine1 { get; set; } 
        public string? AddressLine2 { get; set; }
        public string City { get; set; } 
        public string State { get; set; } 
        public string Country { get; set; } 
        public string? PostalCode { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        public Handler()
        {
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {

            return new BaseResult(HttpStatusCode.OK, "KYC Tier Three information submitted successfully.");
        }
    }
}

