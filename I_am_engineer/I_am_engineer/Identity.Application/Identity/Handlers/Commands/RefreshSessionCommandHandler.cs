using I_am_engineer.Identity.Application.Identity.Commands;
using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Commands;

public sealed class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthTokensResponse>
{
    public Task<AuthTokensResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
