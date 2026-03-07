namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record PasswordChanged(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "PasswordChanged";
    public string Description { get; } = "User password was changed.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
