namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record UserLockedOut(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "UserLockedOut";
    public string Description { get; } = "User account was locked out due to failed sign-ins.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
