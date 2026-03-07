namespace I_am_engineer.Identity.Domain.Events;

public sealed record UserRegistered(Guid UserId, string Email) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
