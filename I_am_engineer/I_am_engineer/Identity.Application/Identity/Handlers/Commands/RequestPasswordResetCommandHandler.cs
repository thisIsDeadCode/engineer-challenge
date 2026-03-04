using I_am_engineer.Identity.Application.Identity.Commands;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Commands;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    public Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
