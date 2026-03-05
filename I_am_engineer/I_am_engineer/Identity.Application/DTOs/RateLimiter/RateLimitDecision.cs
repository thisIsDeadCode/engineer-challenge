namespace I_am_engineer.Identity.Application.DTOs.RateLimiter;

public sealed record RateLimitDecision(
    bool Allowed,
    int RemainingAttempts,
    TimeSpan? RetryAfter,
    int MaxAttempts,
    TimeSpan Window);
