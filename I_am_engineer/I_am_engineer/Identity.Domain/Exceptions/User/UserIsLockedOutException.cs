using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIsLockedOutException : DomainException
{
    public UserIsLockedOutException()
        : base("The user is temporarily locked out.")
    {
    }
}
