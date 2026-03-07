namespace I_am_engineer.Identity.Domain.Events;

public interface IEventEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
