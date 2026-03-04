namespace I_am_engineer.Identity.Application.Identity.Requests;

public sealed record LoginRequest(string Email, string Password, string? DeviceId);
