namespace I_am_engineer.Identity.Domain.ValueObjects;

public record RefreshToken(string Value, DateTimeOffset ExpiresAt);
