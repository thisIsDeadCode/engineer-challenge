using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIsInactiveException : DomainException
{
    public UserIsInactiveException()
        : base("The user is inactive.")
    {
    }
}
