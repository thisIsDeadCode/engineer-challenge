using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, BaseResponse>
{
    public Task<BaseResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
