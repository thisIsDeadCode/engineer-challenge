using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenGenerator passwordResetTokenGenerator) : IRequestHandler<RequestPasswordResetCommand, BaseResponse>
{
    private static readonly PasswordRecoveryPolicy PasswordRecoveryPolicy = new();

    public async Task<BaseResponse> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return new BaseResponse(true, null);
            }

            if (!PasswordRecoveryPolicy.CanRequestReset(user.PasswordResetToken, DateTimeOffset.UtcNow))
            {
                return new BaseResponse(false, "Password reset request is too frequent. Please try again later.");
            }

            user.IssuePasswordResetToken(passwordResetTokenGenerator);

            var isSaved = await userRepository.SaveAsync(user, cancellationToken);
            if (!isSaved)
            {
                return new BaseResponse(false, "Failed to create password reset request.");
            }

            return new BaseResponse(true, null);
        }
        catch (DomainException ex)
        {
            return new BaseResponse(false, ex.Message);
        }
        catch
        {
            return new BaseResponse(false, "Something went wrong.");
        }
    }
}
