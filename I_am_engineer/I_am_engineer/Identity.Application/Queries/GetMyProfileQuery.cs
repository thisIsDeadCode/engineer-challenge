using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Queries;

public sealed record GetMyProfileQuery : IRequest<MyProfileResponse>;
