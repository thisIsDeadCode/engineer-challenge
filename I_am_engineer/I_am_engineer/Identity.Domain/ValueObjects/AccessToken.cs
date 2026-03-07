namespace I_am_engineer.Identity.Domain.ValueObjects;

public record AccessToken(string Value, DateTimeOffset ExpiresAt);
