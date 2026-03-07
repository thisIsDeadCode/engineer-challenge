using I_am_engineer.Identity.Domain.Aggregates;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IUserRegistrationRepository
{
    Task<bool> SaveUserAndSessionAsync(User user, Session session, CancellationToken cancellationToken);
}
