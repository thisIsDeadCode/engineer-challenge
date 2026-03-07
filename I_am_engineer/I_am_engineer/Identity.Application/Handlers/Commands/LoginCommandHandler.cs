using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using I_am_engineer.Identity.Domain.Aggregates;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IRequestHandler<LoginCommand, AuthTokensResponse>
{
    private static readonly LockoutPolicy LockoutPolicy = new();
    private static readonly SessionPolicy SessionPolicy = new();

    public async Task<AuthTokensResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null || user.PasswordHash is null)
            {
                return CreateInvalidCredentialsResponse();
            }

            if (!passwordHasher.Verify(request.Password, user.PasswordHash.Value))
            {
                user.RecordFailedLoginAttempt(LockoutPolicy);
                await userRepository.SaveAsync(user, cancellationToken);

                return CreateInvalidCredentialsResponse();
            }

            user.RecordSuccessfulLogin(LockoutPolicy);

            var session = await sessionRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (session is null)
            {
                SessionPolicy.EnsureCanOpenSession(activeSessionsCount: 0);
                session = Session.Create(user.Id, tokenGenerator);
            }
            else if (!session.IsActive)
            {
                session = Session.Create(user.Id, tokenGenerator);
            }
            else
            {
                session.Rotate(tokenGenerator);
            }

            var isUserSaved = await userRepository.SaveAsync(user, cancellationToken);
            if (!isUserSaved)
            {
                return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Failed to update user state.");
            }

            var isSessionSaved = await sessionRepository.SaveAsync(session, cancellationToken);
            if (!isSessionSaved)
            {
                return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Failed to create user session.");
            }

            return new AuthTokensResponse(
                session.AccessToken.Value,
                session.RefreshToken.Value,
                session.AccessToken.ExpiresAt,
                session.RefreshToken.ExpiresAt,
                true,
                null);
        }
        catch (DomainException ex)
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, ex.Message);
        }
        catch
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Something went wrong.");
        }
    }

    private static AuthTokensResponse CreateInvalidCredentialsResponse()
    {
        return new AuthTokensResponse(
            string.Empty,
            string.Empty,
            DateTimeOffset.MinValue,
            DateTimeOffset.MinValue,
            false,
            "Invalid email or password.");
    }
}
