namespace I_am_engineer.Identity.Application.DTOs.SessionRepository;

public sealed record SessionDto(
    Guid SessionId,
    Guid UserId,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string? DeviceId,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
