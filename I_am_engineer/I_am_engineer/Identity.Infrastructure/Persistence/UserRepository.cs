using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands.Events;
using I_am_engineer.Identity.Application.DTOs.UserRepository;
using I_am_engineer.Identity.Domain.Aggregates;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class UserRepository(IConfiguration configuration, ISender sender)
    : SqlRepository(configuration), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await ExecuteReadAsync(async connection =>
        {
            var userCredentials = await connection.QuerySingleOrDefaultAsync<UserCredentialsDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserCredentialsByEmail",
                parameters: new { Email = email },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            if (userCredentials is null)
            {
                return null;
            }

            var resetToken = await connection.QuerySingleOrDefaultAsync<UserOneTimePasswordResetTokenDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserOneTimePasswordResetToken",
                parameters: new { UserId = userCredentials.UserId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return RestoreAggregate(userCredentials, resetToken);
        }, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await ExecuteReadAsync(async connection =>
        {
            var userCredentials = await connection.QuerySingleOrDefaultAsync<UserCredentialsDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserCredentialsById",
                parameters: new { UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            if (userCredentials is null)
            {
                return null;
            }

            var resetToken = await connection.QuerySingleOrDefaultAsync<UserOneTimePasswordResetTokenDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserOneTimePasswordResetToken",
                parameters: new { UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return RestoreAggregate(userCredentials, resetToken);
        }, cancellationToken);
    }

    public async Task<bool> SaveAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!user.IsChanged)
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

                return true;
            }

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

            return true;
        }, cancellationToken);

        if (isSaved)
        {
            await sender.Send(new ProcessDomainEventsCommand([user]), cancellationToken);
        }

        return isSaved;
    }

    private static User RestoreAggregate(UserCredentialsDto credentials, UserOneTimePasswordResetTokenDto? resetToken)
    {
        return User.Restore(
            id: credentials.UserId,
            email: credentials.Email,
            passwordHash: credentials.PasswordHash,
            passwordResetTokenValue: resetToken?.ResetToken,
            passwordResetTokenIsUsed: resetToken?.IsUsed,
            passwordResetTokenExpiresAt: resetToken?.ExpiresAt,
            passwordResetTokenIssuedAt: resetToken?.CreatedAtUtc,
            failedLoginAttempts: credentials.CurrentFailedAttempts,
            lockedUntilUtc: credentials.LockedUntil,
            isActive: credentials.IsActive,
            session: null,
            createdAtUtc: credentials.CreatedAtUtc,
            updatedAtUtc: credentials.UpdatedAtUtc);
    }
}
