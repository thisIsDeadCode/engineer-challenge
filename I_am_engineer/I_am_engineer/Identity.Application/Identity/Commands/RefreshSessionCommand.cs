using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Commands;

public sealed record RefreshSessionCommand(string RefreshToken) : IRequest<AuthTokensResponse>;
