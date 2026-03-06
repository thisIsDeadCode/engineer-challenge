using I_am_engineer.Identity.Domain.Exceptions.LockoutPolicy;

namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class LockoutPolicy
{
    public const int DefaultMaxFailedAttempts = 5;

    public static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromMinutes(15);

    public int MaxFailedAttempts { get; }

    public TimeSpan LockoutDuration { get; }

    public LockoutPolicy()
        : this(DefaultMaxFailedAttempts, DefaultLockoutDuration)
    {
    }

    public LockoutPolicy(int maxFailedAttempts, TimeSpan lockoutDuration)
    {
        if (maxFailedAttempts <= 0)
        {
            throw new LockoutPolicyInvalidConfigurationException(nameof(maxFailedAttempts), "Max failed attempts must be greater than zero.");
        }

        if (lockoutDuration <= TimeSpan.Zero)
        {
            throw new LockoutPolicyInvalidConfigurationException(nameof(lockoutDuration), "Lockout duration must be greater than zero.");
        }

        MaxFailedAttempts = maxFailedAttempts;
        LockoutDuration = lockoutDuration;
    }

    public bool ShouldLockout(int failedAttempts)
    {
        if (failedAttempts < 0)
        {
            throw new LockoutPolicyInvalidFailedAttemptsException();
        }

        return failedAttempts >= MaxFailedAttempts;
    }

    public DateTimeOffset CalculateLockoutEnd(DateTimeOffset lockedAt)
    {
        return lockedAt + LockoutDuration;
    }

    public bool IsLockedOut(DateTimeOffset? lockedUntil, DateTimeOffset now)
    {
        return lockedUntil.HasValue && lockedUntil.Value > now;
    }
}
