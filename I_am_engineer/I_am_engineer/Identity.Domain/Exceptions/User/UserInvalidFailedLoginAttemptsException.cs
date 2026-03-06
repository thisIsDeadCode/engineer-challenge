namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserInvalidFailedLoginAttemptsException : Exception
{
    public UserInvalidFailedLoginAttemptsException(int failedLoginAttempts)
        : base($"Failed login attempts cannot be negative. Actual value: {failedLoginAttempts}.")
    {
    }
}
