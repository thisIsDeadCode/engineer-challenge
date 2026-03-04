using I_am_engineer.Identity.Application.Identity.Commands;
using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Commands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokensResponse>
{
    public Task<AuthTokensResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
