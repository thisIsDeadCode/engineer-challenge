using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.PasswordHasher;

public sealed class PasswordHashIsRequiredException : InfrastructureException
{
    public PasswordHashIsRequiredException()
        : base("Password hash cannot be null, empty, or whitespace.")
    {
    }
}
