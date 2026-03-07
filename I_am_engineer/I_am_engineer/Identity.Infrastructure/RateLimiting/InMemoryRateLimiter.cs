using System.Collections.Concurrent;
using System.Threading;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Infrastructure.Exceptions.InMemoryRateLimiter;

namespace I_am_engineer.Identity.Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private static readonly TimeSpan DefaultStaleEntryLifetime = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<string, CounterState> _counters = new();
    private readonly Func<DateTimeOffset> _utcNowProvider;
    private readonly int _cleanupEveryNOperations;
    private readonly TimeSpan _staleEntryLifetime;

    private int _operationsSinceCleanup;

    public InMemoryRateLimiter()
        : this(() => DateTimeOffset.UtcNow, cleanupEveryNOperations: 256, staleEntryLifetime: DefaultStaleEntryLifetime)
    {
    }

    public InMemoryRateLimiter(
        Func<DateTimeOffset> utcNowProvider,
        int cleanupEveryNOperations,
        TimeSpan staleEntryLifetime)
    {
        _utcNowProvider = utcNowProvider ?? throw new ArgumentNullException(nameof(utcNowProvider));

        if (cleanupEveryNOperations <= 0)
        {
            throw new RateLimiterInvalidConfigurationException(nameof(cleanupEveryNOperations), "Cleanup interval must be greater than zero.");
        }

        if (staleEntryLifetime <= TimeSpan.Zero)
        {
            throw new RateLimiterInvalidConfigurationException(nameof(staleEntryLifetime), "Stale entry lifetime must be greater than zero.");
        }

        _cleanupEveryNOperations = cleanupEveryNOperations;
        _staleEntryLifetime = staleEntryLifetime;
    }

    public RateLimitDecision Allow(string key, int maxAttempts, TimeSpan window)
    {
        ValidateInput(key, maxAttempts, window);

        var now = _utcNowProvider();
        CleanupIfNeeded(now);

        var state = _counters.GetOrAdd(key, _ => new CounterState(now));

        lock (state.SyncRoot)
        {
            state.LastSeenAt = now;
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

        var now = _utcNowProvider();
        CleanupIfNeeded(now);

        var state = _counters.GetOrAdd(key, _ => new CounterState(now));

        lock (state.SyncRoot)
        {
            state.LastSeenAt = now;
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

    private void CleanupIfNeeded(DateTimeOffset now)
    {
        var operationNumber = Interlocked.Increment(ref _operationsSinceCleanup);
        if (operationNumber % _cleanupEveryNOperations != 0)
        {
            return;
        }

        foreach (var pair in _counters)
        {
            var state = pair.Value;
            var isStale = false;

            lock (state.SyncRoot)
            {
                if (now - state.LastSeenAt >= _staleEntryLifetime)
                {
                    isStale = true;
                }
            }

            if (isStale)
            {
                _counters.TryRemove(pair.Key, out _);
            }
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
        public CounterState(DateTimeOffset now)
        {
            WindowStartedAt = now;
            LastSeenAt = now;
        }

        public object SyncRoot { get; } = new();

        public DateTimeOffset WindowStartedAt { get; set; }

        public DateTimeOffset LastSeenAt { get; set; }

        public int Attempts { get; set; }
    }
}
