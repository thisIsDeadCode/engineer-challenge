namespace I_am_engineer.Identity.Application.DTOs.UserRepository;

public sealed record PasswordResetTokenDto(Guid UserId, string ResetToken, DateTimeOffset ExpiresAt, bool IsUsed);
