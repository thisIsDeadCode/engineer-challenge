namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionRevoked(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}