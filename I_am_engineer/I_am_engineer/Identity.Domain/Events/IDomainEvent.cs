namespace I_am_engineer.Identity.Domain.Events;

public interface IDomainEvent
{
    Guid UserId { get; }
    string Name { get; }
    string Description { get; }
    DateTimeOffset OccurredAtUtc { get; }
}
