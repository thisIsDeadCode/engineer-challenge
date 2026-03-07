using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class RefreshSessionCommandHandler(
    ISessionRepository sessionRepository,
    ITokenGenerator tokenGenerator) : IRequestHandler<RefreshSessionCommand, AuthTokensResponse>
{
    public async Task<AuthTokensResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await sessionRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (session is null || !session.IsActive || session.RefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return new AuthTokensResponse(
                    string.Empty,
                    string.Empty,
                    DateTimeOffset.MinValue,
                    DateTimeOffset.MinValue,
                    false,
                    "Session not found or refresh token is invalid.");
            }

            session.Rotate(tokenGenerator);

            var isSaved = await sessionRepository.SaveAsync(session, cancellationToken);
            if (!isSaved)
            {
                return new AuthTokensResponse(
                    string.Empty,
                    string.Empty,
                    DateTimeOffset.MinValue,
                    DateTimeOffset.MinValue,
                    false,
                    "Failed to refresh session.");
            }

            return new AuthTokensResponse(
                session.AccessToken.Value,
                session.RefreshToken.Value,
                session.AccessToken.ExpiresAt,
                session.RefreshToken.ExpiresAt,
                true,
                null);
        }
        catch
        {
            return new AuthTokensResponse(
                string.Empty,
                string.Empty,
                DateTimeOffset.MinValue,
                DateTimeOffset.MinValue,
                false,
                "Something went wrong.");
        }
    }
}
