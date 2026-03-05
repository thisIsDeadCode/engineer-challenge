namespace I_am_engineer.Identity.Application.Abstractions;

public sealed record UserCredentialsDto(Guid UserId, string Email, string PasswordHash, bool IsActive);

public sealed record SessionTokensDto(Guid SessionId, Guid UserId, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

public sealed record PasswordResetTokenDto(Guid UserId, string ResetToken, DateTimeOffset ExpiresAt, bool IsUsed);

public sealed record UserProfileDto(Guid UserId, string Email, string DisplayName);
