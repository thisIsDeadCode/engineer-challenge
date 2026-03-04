using MediatR;

namespace I_am_engineer.Identity.Application.Commands;

public sealed record LogoutCommand(Guid SessionId) : IRequest<Unit>;
