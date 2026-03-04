using I_am_engineer.Identity.Application.Commands;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class ConfirmPasswordResetCommandHandler : IRequestHandler<ConfirmPasswordResetCommand, Unit>
{
    public Task<Unit> Handle(ConfirmPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
