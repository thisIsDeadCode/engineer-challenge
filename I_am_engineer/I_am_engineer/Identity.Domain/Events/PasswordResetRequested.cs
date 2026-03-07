namespace I_am_engineer.Identity.Domain.Events;

public sealed record PasswordResetRequested(Guid UserId, string ResetToken, DateTimeOffset ExpiresAtUtc) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
