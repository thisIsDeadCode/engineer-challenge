using I_am_engineer.Identity.Application.Identity.Queries;
using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Handlers.Queries;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileResponse>
{
    public Task<MyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
