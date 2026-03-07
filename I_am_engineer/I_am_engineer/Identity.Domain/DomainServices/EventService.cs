using I_am_engineer.Identity.Domain.Events;

namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class EventService
{
    private readonly List<IDomainEvent> _buffer = [];

    public void Save(IEnumerable<IDomainEvent> domainEvents)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        _buffer.AddRange(domainEvents);
        DeliverBufferedEvents();
    }

    private void DeliverBufferedEvents()
    {
        foreach (var domainEvent in _buffer)
        {
            Console.WriteLine(
                "Domain event delivered: {0}; userId: {1}; occurredAtUtc: {2:O}",
                domainEvent.GetType().Name,
                domainEvent.UserId,
                domainEvent.OccurredAtUtc);
        }

        _buffer.Clear();
    }
}
