namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIdIsRequiredException : Exception
{
    public UserIdIsRequiredException()
        : base("User id must not be empty.")
    {
    }
}
