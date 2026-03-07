namespace I_am_engineer.Identity.Domain.Events;

public sealed record SessionRotated(Guid SessionId, Guid UserId, DateTimeOffset RefreshTokenExpiresAt) : IDomainEvent;
