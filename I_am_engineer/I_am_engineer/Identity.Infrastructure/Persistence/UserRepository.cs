using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.UserRepository;
using I_am_engineer.Identity.Domain.Aggregates;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("IdentityDb")
        ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured.");

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

        return await ExecuteInTransactionAsync(async (connection, transaction) =>
        {
            var existingUser = await connection.QuerySingleOrDefaultAsync<UserCredentialsDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetUserCredentialsById",
                parameters: new { UserId = user.Id },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (existingUser is null)
            {
                var created = await connection.ExecuteAsync(new CommandDefinition(
                    commandText: "dbo.usp_Identity_CreateUser",
                    parameters: new { UserId = user.Id, Email = user.Email.Value, PasswordHash = user.PasswordHash!.Value },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                if (created <= 0)
                {
                    return false;
                }
            }

            var updated = await connection.ExecuteAsync(new CommandDefinition(
                commandText: "dbo.usp_Identity_UpdateUserAggregateState",
                parameters: new
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash!.Value,
                    CurrentFailedAttempts = user.FailedLoginAttempts,
                    LockedUntil = user.LockedUntilUtc,
                    IsActive = user.IsActive
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (updated <= 0)
            {
                return false;
            }

            if (user.PasswordResetToken is null)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    commandText: "dbo.usp_Identity_ClearUserOneTimePasswordResetToken",
                    parameters: new { UserId = user.Id },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                return true;
            }

            var tokenAffected = await connection.ExecuteAsync(new CommandDefinition(
                commandText: "dbo.usp_Identity_SaveUserOneTimePasswordResetToken",
                parameters: new
                {
                    UserId = user.Id,
                    ResetToken = user.PasswordResetToken.Value,
                    ExpiresAt = user.PasswordResetToken.ExpiresAt,
                    IsUsed = user.PasswordResetToken.IsUsed
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            return tokenAffected >= 0;
        }, cancellationToken);
    }

    private async Task<T> ExecuteReadAsync<T>(Func<SqlConnection, Task<T>> action, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await action(connection);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> action, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken) as SqlTransaction
            ?? throw new InvalidOperationException("Failed to start SQL transaction.");

        try
        {
            var result = await action(connection, transaction);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
