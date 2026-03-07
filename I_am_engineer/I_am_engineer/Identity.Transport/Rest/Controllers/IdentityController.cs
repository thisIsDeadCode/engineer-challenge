using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Queries;
using I_am_engineer.Identity.Application.Requests;
using I_am_engineer.Identity.Transport.Extensions;
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
        var response = await sender.Send(new CreateUserCommand(request.Email, request.Password, request.ConfirmPassword), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new LoginCommand(request.Email, request.Password, request.DeviceId), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpPost("sessions/refresh")]
    public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new RefreshSessionCommand(request.RefreshToken), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpDelete("Logout/{sessionId:guid}")]
    public async Task<IActionResult> Logout([FromRoute] Guid sessionId, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new LogoutCommand(sessionId), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpPost("password-resets")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new RequestPasswordResetCommand(request.Email), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpPost("password-resets/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword), cancellationToken);
        return Ok(response.ToApiResponse());
    }

    [HttpGet("sessions/me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(response.ToApiResponse());
    }
}
