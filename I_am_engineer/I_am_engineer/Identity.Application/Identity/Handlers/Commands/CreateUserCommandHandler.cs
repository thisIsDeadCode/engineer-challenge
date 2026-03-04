using I_am_engineer.Identity.Application.Identity.Commands;
using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Commands;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AuthTokensResponse>
{
    public Task<AuthTokensResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
