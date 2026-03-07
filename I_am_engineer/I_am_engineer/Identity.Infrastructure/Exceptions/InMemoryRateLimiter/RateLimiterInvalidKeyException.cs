using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.InMemoryRateLimiter;

public sealed class RateLimiterInvalidKeyException : InfrastructureException
{
    public RateLimiterInvalidKeyException()
        : base("Rate limit key must not be empty.")
    {
    }
}
