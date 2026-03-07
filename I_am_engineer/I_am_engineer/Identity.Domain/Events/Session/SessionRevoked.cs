namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionRevoked(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "SessionRevoked";
    public string Description { get; } = "User session was revoked.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
