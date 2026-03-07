using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Responses;
using I_am_engineer.Identity.Domain.Aggregates;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions;
using MediatR;

namespace I_am_engineer.Identity.Application.Handlers.Commands;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUserRegistrationRepository userRegistrationRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IRequestHandler<CreateUserCommand, AuthTokensResponse>
{
    private static readonly PasswordPolicy PasswordPolicy = new();
    private static readonly SessionPolicy SessionPolicy = new();

    public async Task<AuthTokensResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
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

            var user = User.CreateNew(request.Email, request.Password, passwordHasher, PasswordPolicy);

            SessionPolicy.EnsureCanOpenSession(activeSessionsCount: 0);
            var session = Session.Create(user.Id, tokenGenerator);

            var isSaved = await userRegistrationRepository.SaveUserAndSessionAsync(user, session, cancellationToken);
            if (!isSaved)
            {
                return new AuthTokensResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, DateTimeOffset.MinValue, false, "Failed to create user.");
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
}
