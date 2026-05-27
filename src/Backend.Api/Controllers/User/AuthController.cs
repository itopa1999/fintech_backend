using System.Net;
using Backend.Application.Commands;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
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
    public async Task<ActionResult<BaseResult>> RegisterUSer([FromBody] RegisterUserDto dto)
    {

        var command = new RegisterUserCommand.Command
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = dto.Password
        };

        var result = await _mediator.Send(command);

        return result.ToActionResult();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResult<AuthResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> LoginUser([FromBody] LoginUserDto dto)
    {
        var command = new LoginUserCommand.Command
        {
            Email = dto.Email,
            Password = dto.Password
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}