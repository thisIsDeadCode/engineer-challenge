namespace I_am_engineer.Identity.Infrastructure.Exceptions.InMemoryRateLimiter;

public sealed class RateLimiterInvalidKeyException : Exception
{
    public RateLimiterInvalidKeyException()
        : base("Rate limit key must not be empty.")
    {
    }
}
