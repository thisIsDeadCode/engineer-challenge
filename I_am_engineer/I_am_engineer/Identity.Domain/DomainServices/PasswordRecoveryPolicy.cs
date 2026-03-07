using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class PasswordRecoveryPolicy
{
    public const int DefaultMaxRequests = 1;

    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromMinutes(15);

    public PasswordRecoveryPolicy()
        : this(DefaultMaxRequests, DefaultCooldown)
    {
    }

    public PasswordRecoveryPolicy(int maxRequests, TimeSpan cooldown)
    {
        if (maxRequests <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRequests), "Max requests must be greater than zero.");
        }

        if (cooldown <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(cooldown), "Cooldown must be greater than zero.");
        }

        MaxRequests = maxRequests;
        Cooldown = cooldown;
    }

    public int MaxRequests { get; }

    public TimeSpan Cooldown { get; }

    public bool IsTokenActive(PasswordResetToken? token, DateTimeOffset now)
    {
        return token is not null && !token.IsUsed && token.ExpiresAt > now;
    }

    public bool CanRequestReset(PasswordResetToken? token, DateTimeOffset now)
    {
        if (token is null)
        {
            return true;
        }

        var minInterval = TimeSpan.FromTicks(Cooldown.Ticks / MaxRequests);
        return now >= token.IssuedAt.Add(minInterval);
    }
}
