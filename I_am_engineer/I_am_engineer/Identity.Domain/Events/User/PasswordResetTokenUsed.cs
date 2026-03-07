namespace I_am_engineer.Identity.Domain.Events.User;

public sealed record PasswordResetTokenUsed(Guid UserId) : IDomainEvent
{
    public string Name { get; } = "PasswordResetTokenUsed";
    public string Description { get; } = "Password reset token was used.";
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
