using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Queries;

public sealed record GetMyProfileQuery : IRequest<MyProfileResponse>;
