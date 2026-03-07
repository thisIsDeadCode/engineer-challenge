namespace I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

public sealed class SessionPolicyInvalidConfigurationException(string parameterName, string message)
    : ArgumentOutOfRangeException(parameterName, message)
{
}
