namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIsLockedOutException : Exception
{
    public UserIsLockedOutException()
        : base("The user is temporarily locked out.")
    {
    }
}
