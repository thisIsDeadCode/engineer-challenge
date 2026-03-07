using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

namespace Identity.Tests;

public sealed class SessionPolicyTests
{
    [Fact]
    public void Constructor_DefaultsToSingleSessionAndReuseDisabled()
    {
        var policy = new SessionPolicy();

        Assert.Equal(1, policy.MaxSessionsPerUser);
        Assert.False(policy.AllowRefreshTokenReuse);
    }

    [Fact]
    public void EnsureCanOpenSession_Throws_WhenLimitReached()
    {
        var policy = new SessionPolicy(maxSessionsPerUser: 1, reuseDetectionWindow: TimeSpan.Zero, allowRefreshTokenReuse: false);

        Assert.Throws<SessionPolicyViolationException>(() => policy.EnsureCanOpenSession(activeSessionsCount: 1));
    }

    [Fact]
    public void CanReuseRefreshToken_ReturnsTrue_WhenAllowedAndWithinWindow()
    {
        var policy = new SessionPolicy(maxSessionsPerUser: 1, reuseDetectionWindow: TimeSpan.FromSeconds(30), allowRefreshTokenReuse: true);
        var rotatedAt = DateTimeOffset.UtcNow;

        var canReuse = policy.CanReuseRefreshToken(rotatedAt, rotatedAt.AddSeconds(10));

        Assert.True(canReuse);
    }

    [Fact]
    public void EnsureCanRotateSession_Throws_WhenRefreshTokenExpired()
    {
        var policy = new SessionPolicy();
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<SessionPolicyViolationException>(() => policy.EnsureCanRotateSession(now.AddMinutes(-1), now));
    }
}
