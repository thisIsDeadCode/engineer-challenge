namespace I_am_engineer.Identity.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAtUtc { get; }
}
