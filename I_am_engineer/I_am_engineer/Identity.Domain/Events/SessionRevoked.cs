namespace I_am_engineer.Identity.Domain.Events;

public sealed record SessionRevoked(Guid SessionId, Guid UserId) : IDomainEvent;
