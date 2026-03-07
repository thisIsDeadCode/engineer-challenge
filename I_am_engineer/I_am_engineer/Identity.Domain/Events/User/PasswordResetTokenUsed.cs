namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record PasswordResetTokenUsed(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
