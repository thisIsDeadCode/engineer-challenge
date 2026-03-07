using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserInvalidFailedLoginAttemptsException : DomainException
{
    public UserInvalidFailedLoginAttemptsException(int failedLoginAttempts)
        : base($"Failed login attempts cannot be negative. Actual value: {failedLoginAttempts}.")
    {
    }
}
