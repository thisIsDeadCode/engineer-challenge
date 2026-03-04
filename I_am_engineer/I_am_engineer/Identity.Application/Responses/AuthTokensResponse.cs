namespace I_am_engineer.Identity.Application.Responses;

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
