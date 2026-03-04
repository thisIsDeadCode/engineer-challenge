using I_am_engineer.Identity.Application.Commands;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    public Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
