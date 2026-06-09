using System.Net;
using Backend.Application.Common.Results;
using Backend.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Extensions;
using static Backend.Application.BBL.Commands.Auth.RegisterUserCommand;
using Backend.Application.BBL.Commands.Auth;
using static Backend.Application.BBL.Commands.Auth.RefreshTokenCommand;
using Microsoft.AspNetCore.Authorization;
using Backend.Api.Controllers.Shared;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(BaseResult<ResponseDto>), (int)HttpStatusCode.OK)]
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

    [HttpPost("verify-token")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> VerifyToken([FromBody] VerifyTokenDto dto, CancellationToken cancellationToken)
    {
        var command = new VerifyUserCommand.Command
        {
            UserId = dto.UserId,
            Token = dto.Token
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("resend-token")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> ResendToken([FromBody] ResendTokenDto dto, CancellationToken cancellationToken)
    {
        var command = new ResendTokenCommand.Command
        {
            Email = dto.Email.Trim()
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

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(BaseResult<RefreshRespondDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> RefreshToken([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand.Command
        {
            RefreshToken = request.RefreshToken.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();

    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> Logout([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand.Command
        {
            RefreshToken = request.RefreshToken.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand.Command
        {
            Email = dto.Email.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("confirm-forgot-password")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> ConfirmForgotPassword([FromBody] ConfirmForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var command = new ConfirmForgotPasswordCommand.Command
        {
            Token = dto.Token.Trim(),
            NewPassword = dto.NewPassword.Trim(),
            ConfirmPassword = dto.ConfirmPassword.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<BaseResult>> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        var command = new ChangePasswordCommand.Command
        {
            UserId = userId.Value,
            CurrentPassword = dto.CurrentPassword.Trim(),
            NewPassword = dto.NewPassword.Trim(),
            ConfirmPassword = dto.ConfirmPassword.Trim()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}