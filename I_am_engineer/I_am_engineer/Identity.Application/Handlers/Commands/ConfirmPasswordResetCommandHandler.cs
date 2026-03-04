using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class ConfirmPasswordResetCommandHandler : IRequestHandler<ConfirmPasswordResetCommand, BaseResponse>
{
    public Task<BaseResponse> Handle(ConfirmPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
