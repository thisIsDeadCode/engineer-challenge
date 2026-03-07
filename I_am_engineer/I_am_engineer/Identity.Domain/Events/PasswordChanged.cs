namespace I_am_engineer.Identity.Domain.Events;

public sealed record PasswordChanged(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
