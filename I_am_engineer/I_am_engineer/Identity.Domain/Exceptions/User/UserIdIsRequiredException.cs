using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.User;

public sealed class UserIdIsRequiredException : DomainException
{
    public UserIdIsRequiredException()
        : base("User id must not be empty.")
    {
    }
}
