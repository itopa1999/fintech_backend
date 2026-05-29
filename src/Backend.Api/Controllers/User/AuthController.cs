using System.Net;
using Backend.Application.Commands;
using Backend.Application.Common.Results;
using Backend.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Extensions;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> RegisterUSer([FromBody] RegisterUserDto dto, CancellationToken cancellationToken)
    {

        var command = new RegisterUserCommand.Command
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Password = dto.Password.Trim(),
            RoleType = dto.RoleType
        };

        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResult<AuthResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> LoginUser([FromBody] LoginUserDto dto, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand.Command
        {
            Email = dto.Email.Trim(),
            Password = dto.Password.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}