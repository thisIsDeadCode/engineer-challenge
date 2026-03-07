using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands.Events;
using I_am_engineer.Identity.Application.DTOs.SessionRepository;
using I_am_engineer.Identity.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class SessionRepository(IConfiguration configuration, ISender sender)
    : SqlRepository(configuration), ISessionRepository
{
    public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await ExecuteReadAsync(async connection =>
        {
            var session = await connection.QuerySingleOrDefaultAsync<SessionDto>(new CommandDefinition(
                commandText: "dbo.usp_Identity_GetSessionById",
                parameters: new { SessionId = sessionId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return session is null ? null : RestoreAggregate(session);
        }, cancellationToken);
    }

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
            await sender.Send(new ProcessDomainEventsCommand([session]), cancellationToken);
        }

        return isSaved;
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
            isActive: session.IsActive,
            createdAtUtc: session.CreatedAtUtc,
            updatedAtUtc: session.UpdatedAtUtc);
    }
}
