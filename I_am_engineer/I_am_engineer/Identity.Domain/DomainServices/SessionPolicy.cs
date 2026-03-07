using I_am_engineer.Identity.Domain.Exceptions.SessionPolicy;

namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class SessionPolicy
{
    public const int DefaultMaxSessionsPerUser = 1;

    public static readonly TimeSpan DefaultReuseDetectionWindow = TimeSpan.FromMinutes(1);

    public int MaxSessionsPerUser { get; }

    public TimeSpan ReuseDetectionWindow { get; }

    public bool AllowRefreshTokenReuse { get; }

    public SessionPolicy()
        : this(DefaultMaxSessionsPerUser, DefaultReuseDetectionWindow, allowRefreshTokenReuse: false)
    {
    }

    public SessionPolicy(int maxSessionsPerUser, TimeSpan reuseDetectionWindow, bool allowRefreshTokenReuse)
    {
        if (maxSessionsPerUser <= 0)
        {
            throw new SessionPolicyInvalidConfigurationException(nameof(maxSessionsPerUser), "Max sessions per user must be greater than zero.");
        }

        if (reuseDetectionWindow < TimeSpan.Zero)
        {
            throw new SessionPolicyInvalidConfigurationException(nameof(reuseDetectionWindow), "Reuse detection window must be non-negative.");
        }

        MaxSessionsPerUser = maxSessionsPerUser;
        ReuseDetectionWindow = reuseDetectionWindow;
        AllowRefreshTokenReuse = allowRefreshTokenReuse;
    }

    public bool CanOpenSession(int activeSessionsCount)
    {
        if (activeSessionsCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeSessionsCount), "Active sessions count cannot be negative.");
        }

        return activeSessionsCount < MaxSessionsPerUser;
    }

    public void EnsureCanOpenSession(int activeSessionsCount)
    {
        if (CanOpenSession(activeSessionsCount))
        {
            return;
        }

        throw new SessionPolicyViolationException($"The maximum number of active sessions ({MaxSessionsPerUser}) has been reached for this user.");
    }

    public bool IsWithinReuseDetectionWindow(DateTimeOffset lastRotationAtUtc, DateTimeOffset nowUtc)
    {
        if (nowUtc < lastRotationAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(nowUtc), "Current time cannot be earlier than last rotation time.");
        }

        return nowUtc - lastRotationAtUtc <= ReuseDetectionWindow;
    }

    public bool CanReuseRefreshToken(DateTimeOffset lastRotationAtUtc, DateTimeOffset nowUtc)
    {
        return AllowRefreshTokenReuse && IsWithinReuseDetectionWindow(lastRotationAtUtc, nowUtc);
    }

    public bool CanRotateSession(DateTimeOffset refreshTokenExpiresAtUtc, DateTimeOffset nowUtc)
    {
        return nowUtc < refreshTokenExpiresAtUtc;
    }

    public void EnsureCanRotateSession(DateTimeOffset refreshTokenExpiresAtUtc, DateTimeOffset nowUtc)
    {
        if (CanRotateSession(refreshTokenExpiresAtUtc, nowUtc))
        {
            return;
        }

        throw new SessionPolicyViolationException("Session rotation is denied because refresh token TTL has expired.");
    }
}
