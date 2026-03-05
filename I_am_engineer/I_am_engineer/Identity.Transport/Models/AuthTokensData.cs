namespace I_am_engineer.Identity.Transport.Models;

public sealed record AuthTokensData(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
