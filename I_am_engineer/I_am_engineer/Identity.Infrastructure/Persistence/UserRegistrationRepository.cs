using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands.Events;
using I_am_engineer.Identity.Application.DTOs.SessionRepository;
using I_am_engineer.Identity.Application.DTOs.UserRepository;
using I_am_engineer.Identity.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class UserRegistrationRepository(IConfiguration configuration, ISender sender)
    : SqlRepository(configuration), IUserRegistrationRepository
{
    public async Task<bool> SaveUserAndSessionAsync(User user, Session session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(session);

        if (!user.IsChanged && !session.IsChanged)
        {
            return true;
        }

        var isSaved = await ExecuteInTransactionAsync(async (connection, transaction) =>
        {
            var existingUser = await connection.QuerySingleOrDefaultAsync<UserCredentialsDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserCredentialsById",
                parameters: new { UserId = user.Id },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (existingUser is null)
            {
                await EnsureProcedureSucceededAsync(
                    connection,
                    "dbo.usp_Identity_CreateUser",
                    new { UserId = user.Id, Email = user.Email.Value, PasswordHash = user.PasswordHash!.Value },
                    transaction,
                    cancellationToken);
            }

            await EnsureProcedureSucceededAsync(
                connection,
                "dbo.usp_Identity_UpdateUserAggregateState",
                new
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash!.Value,
                    CurrentFailedAttempts = user.FailedLoginAttempts,
                    LockedUntil = user.LockedUntilUtc,
                    IsActive = user.IsActive
                },
                transaction,
                cancellationToken);

            if (user.PasswordResetToken is null)
            {
                await EnsureProcedureSucceededAsync(
                    connection,
                    "dbo.usp_Identity_ClearUserOneTimePasswordResetToken",
                    new { UserId = user.Id },
                    transaction,
                    cancellationToken);
            }
            else
            {
                await EnsureProcedureSucceededAsync(
                    connection,
                    "dbo.usp_Identity_SaveUserOneTimePasswordResetToken",
                    new
                    {
                        UserId = user.Id,
                        ResetToken = user.PasswordResetToken.Value,
                        ExpiresAt = user.PasswordResetToken.ExpiresAt,
                        IsUsed = user.PasswordResetToken.IsUsed
                    },
                    transaction,
                    cancellationToken);
            }

            var existingSession = await connection.QuerySingleOrDefaultAsync<SessionDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetSessionById",
                parameters: new { SessionId = session.Id },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (existingSession is null)
            {
                await EnsureProcedureSucceededAsync(
                    connection,
                    "dbo.usp_Identity_CreateSession",
                    new
                    {
                        SessionId = session.Id,
                        UserId = session.UserId,
                        AccessToken = session.AccessToken.Value,
                        AccessTokenExpiresAt = session.AccessToken.ExpiresAt,
                        RefreshToken = session.RefreshToken.Value,
                        RefreshTokenExpiresAt = session.RefreshToken.ExpiresAt
                    },
                    transaction,
                    cancellationToken);

                return true;
            }

            await EnsureProcedureSucceededAsync(
                connection,
                "dbo.usp_Identity_UpdateSessionAggregateState",
                new
                {
                    SessionId = session.Id,
                    AccessToken = session.AccessToken.Value,
                    AccessTokenExpiresAt = session.AccessToken.ExpiresAt,
                    RefreshToken = session.RefreshToken.Value,
                    RefreshTokenExpiresAt = session.RefreshToken.ExpiresAt,
                    IsActive = session.IsActive
                },
                transaction,
                cancellationToken);

            return true;
        }, cancellationToken);

        if (isSaved)
        {
            await sender.Send(new ProcessDomainEventsCommand([user]), cancellationToken);
            await sender.Send(new ProcessDomainEventsCommand([session]), cancellationToken);
        }

        return isSaved;
    }
}
