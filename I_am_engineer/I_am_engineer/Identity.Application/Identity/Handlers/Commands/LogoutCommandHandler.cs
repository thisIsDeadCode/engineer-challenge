using I_am_engineer.Identity.Application.Identity.Commands;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Commands;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    public Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
