using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.LockoutPolicy;

public sealed class LockoutPolicyInvalidFailedAttemptsException : DomainException
{
    public LockoutPolicyInvalidFailedAttemptsException()
        : base("Failed attempts cannot be negative.")
    {
    }
}
