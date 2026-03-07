namespace I_am_engineer.Identity.Domain.Events;

public sealed record UserLockedOut(Guid UserId, DateTimeOffset? LockedUntilUtc) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
