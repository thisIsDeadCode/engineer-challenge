using System.Reflection;
using I_am_engineer.Identity.Infrastructure.RateLimiting;

namespace Identity.Tests;

public sealed class InMemoryRateLimiterTests
{
    [Fact]
    public void CleanupIfNeeded_RemovesStaleCounters()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var limiter = new InMemoryRateLimiter(() => now, cleanupEveryNOperations: 1, staleEntryLifetime: TimeSpan.FromMinutes(5));

        _ = limiter.Allow("first", maxAttempts: 10, window: TimeSpan.FromMinutes(1));

        now = now.AddMinutes(6);
        _ = limiter.Allow("second", maxAttempts: 10, window: TimeSpan.FromMinutes(1));

        Assert.False(ContainsKey(limiter, "first"));
        Assert.True(ContainsKey(limiter, "second"));
    }

    [Fact]
    public void CleanupIfNeeded_DoesNotRemoveActiveCounters()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var limiter = new InMemoryRateLimiter(() => now, cleanupEveryNOperations: 1, staleEntryLifetime: TimeSpan.FromMinutes(5));

        _ = limiter.Allow("first", maxAttempts: 10, window: TimeSpan.FromMinutes(1));

        now = now.AddMinutes(2);
        _ = limiter.Allow("second", maxAttempts: 10, window: TimeSpan.FromMinutes(1));

        Assert.True(ContainsKey(limiter, "first"));
        Assert.True(ContainsKey(limiter, "second"));
    }

    private static bool ContainsKey(InMemoryRateLimiter limiter, string key)
    {
        var field = typeof(InMemoryRateLimiter).GetField("_counters", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var counters = field.GetValue(limiter);
        Assert.NotNull(counters);

        var containsKeyMethod = counters.GetType().GetMethod("ContainsKey", [typeof(string)]);
        Assert.NotNull(containsKeyMethod);

        var contains = containsKeyMethod.Invoke(counters, [key]);
        Assert.IsType<bool>(contains);

        return (bool)contains;
    }
}
