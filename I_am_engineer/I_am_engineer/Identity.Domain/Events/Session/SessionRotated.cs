namespace I_am_engineer.Identity.Domain.Events.Session;

public sealed record SessionRotated(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "SessionRotated";
    public string Description { get; } = "Session refresh token was rotated.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
