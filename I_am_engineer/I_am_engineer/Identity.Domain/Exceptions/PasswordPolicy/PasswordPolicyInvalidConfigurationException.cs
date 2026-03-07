using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.PasswordPolicy;

public sealed class PasswordPolicyInvalidConfigurationException : DomainException
{
    public PasswordPolicyInvalidConfigurationException(string parameterName, string message)
        : base($"Invalid password policy configuration for '{parameterName}': {message}")
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
