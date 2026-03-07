using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.InMemoryRateLimiter;

public sealed class RateLimiterInvalidConfigurationException : InfrastructureException
{
    public RateLimiterInvalidConfigurationException(string parameterName, string message)
        : base($"Invalid rate limiter configuration for '{parameterName}': {message}")
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
