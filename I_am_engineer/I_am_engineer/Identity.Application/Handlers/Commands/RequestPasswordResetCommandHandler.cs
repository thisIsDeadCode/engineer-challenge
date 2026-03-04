using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, BaseResponse>
{
    public Task<BaseResponse> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
