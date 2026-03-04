using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Commands;

public sealed record ConfirmPasswordResetCommand(string ResetToken, string NewPassword) : IRequest<BaseResponse>;
