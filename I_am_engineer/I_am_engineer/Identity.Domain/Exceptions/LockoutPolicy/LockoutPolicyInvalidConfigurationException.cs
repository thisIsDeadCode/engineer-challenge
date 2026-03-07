using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.LockoutPolicy;

public sealed class LockoutPolicyInvalidConfigurationException : DomainException
{
    public LockoutPolicyInvalidConfigurationException(string parameterName, string message)
        : base($"Invalid lockout policy configuration for '{parameterName}': {message}")
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
