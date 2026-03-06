namespace I_am_engineer.Identity.Domain.Exceptions.LockoutPolicy;

public sealed class LockoutPolicyInvalidFailedAttemptsException : Exception
{
    public LockoutPolicyInvalidFailedAttemptsException()
        : base("Failed attempts cannot be negative.")
    {
    }
}
