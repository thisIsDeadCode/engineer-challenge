using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using I_am_engineer.Identity.Domain.Aggregates;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions.PasswordPolicy;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IRequestHandler<CreateUserCommand, AuthTokensResponse>
{
    private static readonly PasswordPolicy PasswordPolicy = new();
    private static readonly SessionPolicy SessionPolicy = new();
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(2);

    public async Task<AuthTokensResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Passwords do not match.");
        }

        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "User with this email already exists.");
        }

        User user;

        try
        {
            user = User.CreateNew(request.Email, request.Password, passwordHasher, PasswordPolicy);
        }
        catch (PasswordPolicyViolationException ex)
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, ex.Message);
        }

        SessionPolicy.EnsureCanOpenSession(activeSessionsCount: 0);
        var session = Session.Create(user.Id, deviceId: null, RefreshTokenLifetime, tokenGenerator);

        var userSaved = await userRepository.SaveAsync(user, cancellationToken);
        if (!userSaved)
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Failed to save user.");
        }

        var sessionSaved = await sessionRepository.SaveAsync(session, cancellationToken);
        if (!sessionSaved)
        {
            return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Failed to save session.");
        }

        return new AuthTokensResponse(
            session.AccessToken.Value,
            session.RefreshToken.Value,
            session.AccessToken.ExpiresAt,
            session.RefreshToken.ExpiresAt,
            true,
            null);
    }
}
