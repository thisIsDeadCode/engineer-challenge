using I_am_engineer.Identity.Domain.Exceptions;

namespace I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

public sealed class SessionPolicyViolationException(string message)
    : DomainException(message)
{
}
