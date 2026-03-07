using I_am_engineer.Identity.Domain.Aggregates;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> SaveAsync(User user, CancellationToken cancellationToken);
}
