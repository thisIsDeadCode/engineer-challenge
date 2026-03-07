using I_am_engineer.Identity.Application.Commands.Events;
using I_am_engineer.Identity.Domain.DomainServices;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands.Events;

public sealed class ProcessDomainEventsCommandHandler(EventService eventService) : IRequestHandler<ProcessDomainEventsCommand>
{
    public Task Handle(ProcessDomainEventsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var eventsToProcess = request.Entities
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        if (eventsToProcess.Count > 0)
        {
            eventService.Save(eventsToProcess);
        }

        foreach (var entity in request.Entities)
        {
            entity.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}
