namespace I_am_engineer.Identity.Domain.Events;

public sealed record SessionOpened(Guid SessionId, Guid UserId, DateTimeOffset RefreshTokenExpiresAt) : IDomainEvent;
