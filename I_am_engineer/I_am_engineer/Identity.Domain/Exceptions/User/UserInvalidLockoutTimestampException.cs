namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserInvalidLockoutTimestampException : Exception
{
    public UserInvalidLockoutTimestampException()
        : base("Lockout timestamp cannot be earlier than creation timestamp.")
    {
    }
}
