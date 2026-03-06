namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserInvalidPasswordResetExpirationException : Exception
{
    public UserInvalidPasswordResetExpirationException()
        : base("Reset token expiration must be in the future.")
    {
    }
}
