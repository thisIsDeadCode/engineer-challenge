using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AuthTokensResponse>
{
    public Task<AuthTokensResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
