using System.Net;
using Backend.Api.Controllers.Shared;
using MediatR;
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

    
}