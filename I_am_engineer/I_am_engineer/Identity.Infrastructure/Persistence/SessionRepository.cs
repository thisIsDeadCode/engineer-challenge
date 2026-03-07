using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands.Events;
using I_am_engineer.Identity.Application.DTOs.SessionRepository;
using I_am_engineer.Identity.Domain.Aggregates;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class SessionRepository(IConfiguration configuration, ISender sender) : ISessionRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("IdentityDb")
        ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured.");

    public async Task<Session?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await ExecuteReadAsync(async connection =>
        {
            var session = await connection.QuerySingleOrDefaultAsync<SessionDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetSessionByUserId",
                parameters: new { UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return session is null ? null : RestoreAggregate(session);
        }, cancellationToken);
    }

    public async Task<Session?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await ExecuteReadAsync(async connection =>
        {
            var session = await connection.QuerySingleOrDefaultAsync<SessionDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetSessionByEmail",
                parameters: new { Email = email },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return session is null ? null : RestoreAggregate(session);
        }, cancellationToken);
    }

    public async Task<bool> SaveAsync(Session session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.IsChanged)
        {
            return true;
        }

        var isSaved = await ExecuteInTransactionAsync(async (connection, transaction) =>
        {
            var existingSession = await connection.QuerySingleOrDefaultAsync<SessionDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetSessionById",
                parameters: new { SessionId = session.Id },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (existingSession is null)
            {
                var created = await connection.ExecuteAsync(new CommandDefinition(
                    commandText: "dbo.usp_Identity_CreateSession",
                    parameters: new
                    {
                        SessionId = session.Id,
                        UserId = session.UserId,
                        AccessToken = session.AccessToken.Value,
                        AccessTokenExpiresAt = session.AccessToken.ExpiresAt,
                        RefreshToken = session.RefreshToken.Value,
                        RefreshTokenExpiresAt = session.RefreshToken.ExpiresAt,
                        DeviceId = session.DeviceId
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                return created > 0;
            }

            var updated = await connection.ExecuteAsync(new CommandDefinition(
                commandText: "dbo.usp_Identity_UpdateSessionAggregateState",
                parameters: new
                {
                    SessionId = session.Id,
                    AccessToken = session.AccessToken.Value,
                    AccessTokenExpiresAt = session.AccessToken.ExpiresAt,
                    RefreshToken = session.RefreshToken.Value,
                    RefreshTokenExpiresAt = session.RefreshToken.ExpiresAt,
                    IsActive = session.IsActive
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction,
                cancellationToken: cancellationToken));

            return updated > 0;
        }, cancellationToken);

        if (isSaved)
        {
            await sender.Send(new ProcessDomainEventsCommand([session]), cancellationToken);
        }

        return isSaved;
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

    private static Session RestoreAggregate(SessionDto session)
    {
        return Session.Restore(
            id: session.SessionId,
            userId: session.UserId,
            accessToken: session.AccessToken,
            accessTokenExpiresAt: session.AccessTokenExpiresAt,
            refreshToken: session.RefreshToken,
            refreshTokenExpiresAt: session.RefreshTokenExpiresAt,
            deviceId: session.DeviceId,
            isActive: session.IsActive,
            createdAtUtc: session.CreatedAtUtc,
            updatedAtUtc: session.UpdatedAtUtc);
    }
}
