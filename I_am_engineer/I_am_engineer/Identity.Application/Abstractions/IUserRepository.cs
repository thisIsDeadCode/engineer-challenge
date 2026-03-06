using I_am_engineer.Identity.Application.DTOs.UserRepository;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> CreateUserAsync(Guid userId, string email, string passwordHash, CancellationToken cancellationToken);

    Task<UserCredentialsDto?> GetUserCredentialsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> UpdateUserLockoutAsync(Guid userId, int currentFailedAttempts, DateTimeOffset? lockedUntil, CancellationToken cancellationToken);

    Task<UserOneTimePasswordResetTokenDto?> GetUserOneTimePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> SaveUserOneTimePasswordResetTokenAsync(Guid userId, string resetToken, DateTimeOffset expiresAt, CancellationToken cancellationToken);

    Task<bool> ClearUserOneTimePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> CreateSessionAsync(Guid sessionId, Guid userId, string refreshToken, DateTimeOffset refreshTokenExpiresAt, string? deviceId, CancellationToken cancellationToken);

    Task<SessionTokensDto?> RefreshSessionAsync(string refreshToken, string nextRefreshToken, DateTimeOffset nextRefreshTokenExpiresAt, CancellationToken cancellationToken);

    Task<bool> DeactivateSessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<bool> CreatePasswordResetAsync(string email, string resetToken, DateTimeOffset expiresAt, CancellationToken cancellationToken);

    Task<PasswordResetTokenDto?> GetPasswordResetAsync(string resetToken, CancellationToken cancellationToken);

    Task<bool> ConfirmPasswordResetAsync(string resetToken, string newPasswordHash, CancellationToken cancellationToken);

    Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
}
