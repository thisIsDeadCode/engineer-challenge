using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class LogoutCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<LogoutCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
            {
                return new BaseResponse(false, "Session not found.");
            }

            session.Revoke();

            var isSaved = await sessionRepository.SaveAsync(session, cancellationToken);
            if (!isSaved)
            {
                return new BaseResponse(false, "Failed to revoke session.");
            }

            return new BaseResponse(true, null);
        }
        catch
        {
            return new BaseResponse(false, "Something went wrong.");
        }
    }
}
