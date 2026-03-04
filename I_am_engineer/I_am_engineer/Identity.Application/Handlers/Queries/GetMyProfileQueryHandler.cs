using I_am_engineer.Identity.Application.Queries;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Queries;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileResponse>
{
    public Task<MyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
