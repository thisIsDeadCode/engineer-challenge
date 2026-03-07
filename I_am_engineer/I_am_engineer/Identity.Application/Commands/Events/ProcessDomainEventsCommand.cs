using I_am_engineer.Identity.Domain.Events;
using MediatR;

namespace I_am_engineer.Identity.Application.Commands.Events;

public sealed record ProcessDomainEventsCommand(IReadOnlyCollection<IEventEntity> Entities) : IRequest;
