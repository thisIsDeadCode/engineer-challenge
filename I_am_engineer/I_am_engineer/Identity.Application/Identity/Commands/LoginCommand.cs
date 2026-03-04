using I_am_engineer.Identity.Application.Identity.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Commands;

public sealed record LoginCommand(string Email, string Password, string? DeviceId) : IRequest<AuthTokensResponse>;
