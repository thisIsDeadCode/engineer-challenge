using I_am_engineer.Identity.Application.DTOs.RateLimiter;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IRateLimiter
{
    RateLimitDecision Allow(string key, int maxAttempts, TimeSpan window);

    RateLimitDecision IsAllowed(string key, int maxAttempts, TimeSpan window);
}
