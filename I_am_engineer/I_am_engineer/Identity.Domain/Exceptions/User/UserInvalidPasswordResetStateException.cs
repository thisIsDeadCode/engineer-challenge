namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserInvalidPasswordResetStateException : Exception
{
    public UserInvalidPasswordResetStateException()
        : base("Reset token expiration cannot exist without reset token.")
    {
    }
}
