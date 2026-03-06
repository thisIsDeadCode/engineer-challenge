namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIsInactiveException : Exception
{
    public UserIsInactiveException()
        : base("The user is inactive.")
    {
    }
}
