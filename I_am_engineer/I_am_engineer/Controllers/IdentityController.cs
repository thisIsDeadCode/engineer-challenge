using I_am_engineer.Identity.Application.Identity.Commands;
using I_am_engineer.Identity.Application.Identity.Queries;
using I_am_engineer.Identity.Application.Identity.Requests;
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

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.DeviceId);
        var response = await sender.Send(command, cancellationToken);

        return Ok(response);
    }

    [HttpPost("sessions/refresh")]
    public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshSessionCommand(request.RefreshToken);
        var response = await sender.Send(command, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("Logout/{sessionId:guid}")]
    public async Task<IActionResult> Logout([FromRoute] Guid sessionId, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(sessionId);
        await sender.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("password-resets")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        await sender.Send(command, cancellationToken);

        return Accepted();
    }

    [HttpPost("password-resets/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmPasswordResetCommand(request.ResetToken, request.NewPassword);
        await sender.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpGet("sessions/me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery();
        var response = await sender.Send(query, cancellationToken);

        return Ok(response);
    }
}
