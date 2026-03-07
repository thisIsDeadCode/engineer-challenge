namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionOpened(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
