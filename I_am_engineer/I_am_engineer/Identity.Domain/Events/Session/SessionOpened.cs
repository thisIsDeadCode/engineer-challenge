namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionOpened(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "SessionOpened";
    public string Description { get; } = "New user session was opened.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
