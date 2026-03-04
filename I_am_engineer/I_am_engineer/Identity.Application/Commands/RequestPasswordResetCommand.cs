using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Commands;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<BaseResponse>;
