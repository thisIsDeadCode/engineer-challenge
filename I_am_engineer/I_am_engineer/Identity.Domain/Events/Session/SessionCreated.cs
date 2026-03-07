namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionCreated(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "SessionCreated";
    public string Description { get; } = "New user session was created.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
