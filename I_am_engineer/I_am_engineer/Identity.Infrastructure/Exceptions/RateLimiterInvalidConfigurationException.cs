namespace I_am_engineer.Identity.Infrastructure.Exceptions;

public sealed class RateLimiterInvalidConfigurationException : Exception
{
    public RateLimiterInvalidConfigurationException(string parameterName, string message)
        : base($"Invalid rate limiter configuration for '{parameterName}': {message}")
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
