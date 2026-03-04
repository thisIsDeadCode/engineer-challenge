using MediatR;

namespace I_am_engineer.Identity.Application.Identity.Commands;

public sealed record LogoutCommand(Guid SessionId) : IRequest<Unit>;
