using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.PasswordHasher;

public sealed class PasswordIsRequiredException : InfrastructureException
{
    public PasswordIsRequiredException()
        : base("Password cannot be null, empty, or whitespace.")
    {
    }
}
