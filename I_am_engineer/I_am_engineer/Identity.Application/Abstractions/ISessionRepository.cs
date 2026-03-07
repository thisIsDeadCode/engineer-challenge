using I_am_engineer.Identity.Domain.Aggregates;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<Session?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Session?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<bool> SaveAsync(Session session, CancellationToken cancellationToken);
}
