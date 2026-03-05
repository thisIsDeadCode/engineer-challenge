namespace I_am_engineer.Identity.Domain.Exceptions.PasswordHasher;

public sealed class PasswordIsRequiredException : Exception
{
    public PasswordIsRequiredException()
        : base("Password cannot be null, empty, or whitespace.")
    {
    }
}
