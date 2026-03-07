namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record UserDeactivated(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "UserDeactivated";
    public string Description { get; } = "User account was deactivated.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
