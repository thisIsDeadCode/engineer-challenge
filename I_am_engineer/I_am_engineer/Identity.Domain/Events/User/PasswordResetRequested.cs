namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record PasswordResetRequested(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "PasswordResetRequested";
    public string Description { get; } = "Password reset was requested.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
