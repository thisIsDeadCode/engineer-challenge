namespace I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

public sealed class SessionPolicyViolationException(string message)
    : InvalidOperationException(message)
{
}
