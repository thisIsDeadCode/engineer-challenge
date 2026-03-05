namespace I_am_engineer.Identity.Application.DTOs.UserRepository;

public sealed record SessionTokensDto(Guid SessionId, Guid UserId, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);
