using System.Collections.Concurrent;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Infrastructure.Exceptions.InMemoryRateLimiter;

namespace I_am_engineer.Identity.Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, CounterState> _counters = new();

    public RateLimitDecision Allow(string key, int maxAttempts, TimeSpan window)
    {
        ValidateInput(key, maxAttempts, window);

        var state = _counters.GetOrAdd(key, _ => new CounterState());
        var now = DateTimeOffset.UtcNow;

        lock (state.SyncRoot)
        {
            ResetWindowIfExpired(state, now, window);

            if (state.Attempts >= maxAttempts)
            {
                return BuildBlockedDecision(now, state.WindowStartedAt, maxAttempts, window);
            }

            state.Attempts++;
            var remainingAttempts = maxAttempts - state.Attempts;

            return new RateLimitDecision(
                Allowed: true,
                RemainingAttempts: remainingAttempts,
                RetryAfter: null,
                MaxAttempts: maxAttempts,
                Window: window);
        }
    }

    public RateLimitDecision IsAllowed(string key, int maxAttempts, TimeSpan window)
    {
        ValidateInput(key, maxAttempts, window);

        var state = _counters.GetOrAdd(key, _ => new CounterState());
        var now = DateTimeOffset.UtcNow;

        lock (state.SyncRoot)
        {
            ResetWindowIfExpired(state, now, window);

            if (state.Attempts >= maxAttempts)
            {
                return BuildBlockedDecision(now, state.WindowStartedAt, maxAttempts, window);
            }

            return new RateLimitDecision(
                Allowed: true,
                RemainingAttempts: maxAttempts - state.Attempts,
                RetryAfter: null,
                MaxAttempts: maxAttempts,
                Window: window);
        }
    }

    private static RateLimitDecision BuildBlockedDecision(
        DateTimeOffset now,
        DateTimeOffset windowStartedAt,
        int maxAttempts,
        TimeSpan window)
    {
        var retryAfter = window - (now - windowStartedAt);
        if (retryAfter < TimeSpan.Zero)
        {
            retryAfter = TimeSpan.Zero;
        }

        return new RateLimitDecision(
            Allowed: false,
            RemainingAttempts: 0,
            RetryAfter: retryAfter,
            MaxAttempts: maxAttempts,
            Window: window);
    }

    private static void ResetWindowIfExpired(CounterState state, DateTimeOffset now, TimeSpan window)
    {
        if (now - state.WindowStartedAt < window)
        {
            return;
        }

        state.WindowStartedAt = now;
        state.Attempts = 0;
    }

    private static void ValidateInput(string key, int maxAttempts, TimeSpan window)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new RateLimiterInvalidKeyException();
        }

        if (maxAttempts <= 0)
        {
            throw new RateLimiterInvalidConfigurationException(nameof(maxAttempts), "Max attempts must be greater than zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new RateLimiterInvalidConfigurationException(nameof(window), "Window must be greater than zero.");
        }
    }

    private sealed class CounterState
    {
        public object SyncRoot { get; } = new();

        public DateTimeOffset WindowStartedAt { get; set; } = DateTimeOffset.UtcNow;

        public int Attempts { get; set; }
    }
}
