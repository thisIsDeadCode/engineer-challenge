namespace I_am_engineer.Identity.Domain.Exceptions.PasswordHasher;

public sealed class PasswordHashIsRequiredException : Exception
{
    public PasswordHashIsRequiredException()
        : base("Password hash cannot be null, empty, or whitespace.")
    {
    }
}
