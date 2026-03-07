using I_am_engineer.Identity.Application.DTOs.UserRepository;
using I_am_engineer.Identity.Domain.Aggregates;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> SaveAsync(User user, CancellationToken cancellationToken);

    Task<bool> OpenSessionAsync(Guid sessionId, Guid userId, string refreshToken, DateTimeOffset refreshTokenExpiresAt, string? deviceId, CancellationToken cancellationToken);

    Task<SessionTokensDto?> RotateSessionAsync(string refreshToken, string nextRefreshToken, DateTimeOffset nextRefreshTokenExpiresAt, CancellationToken cancellationToken);

    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}
