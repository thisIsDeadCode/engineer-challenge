namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record UserRegistered(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "UserRegistered";
    public string Description { get; } = "New user account was registered.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
