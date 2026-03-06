namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserPasswordIsRequiredException : Exception
{
    public UserPasswordIsRequiredException()
        : base("User password must be set before persisting or authenticating the user.")
    {
    }
}
