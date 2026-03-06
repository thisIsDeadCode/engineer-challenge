namespace I_am_engineer.Identity.Application.DTOs.UserRepository;

public sealed record UserOneTimePasswordResetTokenDto(
    Guid UserId,
    string ResetToken,
    DateTimeOffset ExpiresAt,
    bool IsUsed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
