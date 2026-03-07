using I_am_engineer.Identity.Domain.Exceptions;

namespace I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

public sealed class SessionPolicyInvalidConfigurationException(string parameterName, string message)
    : DomainException($"Parameter '{parameterName}' is invalid. {message}")
{
}
