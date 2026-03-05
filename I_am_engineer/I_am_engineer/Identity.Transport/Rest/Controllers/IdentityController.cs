using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Queries;
using I_am_engineer.Identity.Application.Requests;
using I_am_engineer.Identity.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace I_am_engineer.Identity.Transport.Rest.Controllers;

[ApiController]
[Route("api/v1/identity")]
public class IdentityController(ISender sender) : ControllerBase
{
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.Password);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse<AuthTokensResponse>.Success(response));
        }
        else 
        { 
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceId);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse<AuthTokensResponse>.Success(response));
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpPost("sessions/refresh")]
    public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshSessionCommand(request.RefreshToken);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse<AuthTokensResponse>.Success(response));
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpDelete("Logout/{sessionId:guid}")]
    public async Task<IActionResult> Logout([FromRoute] Guid sessionId, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(sessionId);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse.Success());
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpPost("password-resets")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse.Success());
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpPost("password-resets/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword);
        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse.Success());
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }

    [HttpGet("sessions/me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery();
        var response = await sender.Send(query, cancellationToken);

        if (response.IsSuccess)
        {
            return Ok(ApiResponse<MyProfileResponse>.Success(response));
        }
        else
        {
            return Ok(ApiResponse.Error(response.Message!));
        }
    }
}
