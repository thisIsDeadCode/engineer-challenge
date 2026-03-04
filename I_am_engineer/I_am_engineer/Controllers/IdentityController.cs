using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Queries;
using I_am_engineer.Identity.Application.Requests;
using I_am_engineer.Identity.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace I_am_engineer.Controllers;

[ApiController]
[Route("api/v1/identity")]
public class IdentityController(ISender sender) : ControllerBase
{
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.Password);
        var response = await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<AuthTokensResponse>.Success(response, "User created"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceId);
        var response = await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<AuthTokensResponse>.Success(response, "Login successful"));
    }

    [HttpPost("sessions/refresh")]
    public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshSessionCommand(request.RefreshToken);
        var response = await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<AuthTokensResponse>.Success(response, "Session refreshed"));
    }

    [HttpDelete("Logout/{sessionId:guid}")]
    public async Task<IActionResult> Logout([FromRoute] Guid sessionId, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(sessionId);
        await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<object>.Success(null, "Session logged out"));
    }

    [HttpPost("password-resets")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        await sender.Send(command, cancellationToken);

        return Accepted(ApiResponse<object>.Success(null, "Password reset requested"));
    }

    [HttpPost("password-resets/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword);
        await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<object>.Success(null, "Password reset confirmed"));
    }

    [HttpGet("sessions/me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery();
        var response = await sender.Send(query, cancellationToken);

        return Ok(ApiResponse<MyProfileResponse>.Success(response, "Profile fetched"));
    }
}
