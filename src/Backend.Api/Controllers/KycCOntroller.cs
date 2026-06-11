using System.Net;
using Backend.Api.Controllers.Shared;
using Backend.Api.Extensions;
using Backend.Application.BBL.Commands.Kyc;
using Backend.Application.Common.Results;
using Backend.Application.DTOs.Kyc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/v1/kyc")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class KycController : BaseController
{
    private readonly IMediator _mediator;

    public KycController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create-tier-one")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> CreateKycTierOne([FromBody] CreateKycTierOneDto dto, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        var command = new CreateKycTierOneCommand.Command
        {
            UserId = userId.Value,
            BVN = dto.BVN,
            NIN = dto.NIN
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("create-tier-two")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> CreateKycTierTwo([FromForm] CreateKycTierTwoDto dto, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        var command = new CreateKycTierTwoCommand.Command
        {
            UserId = userId.Value,
            IdDocument = dto.IdDocument,
            Selfie = dto.Selfie
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("create-tier-three")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> CreateKycTierThree([FromBody] CreateKycTierThreeDto dto, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        var command = new CreateKycTierThreeCommand.Command
        {
            UserId = userId.Value,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}