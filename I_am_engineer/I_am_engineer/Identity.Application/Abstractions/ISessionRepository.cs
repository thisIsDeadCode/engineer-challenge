using I_am_engineer.Identity.Domain.Aggregates;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Session?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> SaveAsync(Session session, CancellationToken cancellationToken);
}
