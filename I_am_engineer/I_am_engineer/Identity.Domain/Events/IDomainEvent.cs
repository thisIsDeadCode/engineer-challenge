namespace I_am_engineer.Identity.Domain.Events;

public interface IDomainEvent
{
    Guid UserId { get; }
    DateTimeOffset OccurredAtUtc { get; }
}
