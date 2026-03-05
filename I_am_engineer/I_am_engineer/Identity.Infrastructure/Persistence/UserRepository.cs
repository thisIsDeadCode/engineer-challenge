using System.Data;
using Dapper;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.UserRepository;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public sealed class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("IdentityDb")
        ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured.");

    public async Task<bool> CreateUserAsync(Guid userId, string email, string passwordHash, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            commandText: "dbo.usp_Identity_CreateUser",
            parameters: new { UserId = userId, Email = email, PasswordHash = passwordHash },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<UserCredentialsDto?> GetUserCredentialsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<UserCredentialsDto>(new CommandDefinition(
            commandText: "dbo.usp_Identity_GetUserCredentialsByEmail",
            parameters: new { Email = email },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<bool> CreateSessionAsync(Guid sessionId, Guid userId, string refreshToken, DateTimeOffset refreshTokenExpiresAt, string? deviceId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            commandText: "dbo.usp_Identity_CreateSession",
            parameters: new
            {
                SessionId = sessionId,
                UserId = userId,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                DeviceId = deviceId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<SessionTokensDto?> RefreshSessionAsync(string refreshToken, string nextRefreshToken, DateTimeOffset nextRefreshTokenExpiresAt, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<SessionTokensDto>(new CommandDefinition(
            commandText: "dbo.usp_Identity_RefreshSession",
            parameters: new
            {
                RefreshToken = refreshToken,
                NextRefreshToken = nextRefreshToken,
                NextRefreshTokenExpiresAt = nextRefreshTokenExpiresAt
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<bool> DeactivateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            commandText: "dbo.usp_Identity_DeactivateSession",
            parameters: new { SessionId = sessionId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> CreatePasswordResetAsync(string email, string resetToken, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            commandText: "dbo.usp_Identity_CreatePasswordReset",
            parameters: new { Email = email, ResetToken = resetToken, ExpiresAt = expiresAt },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<PasswordResetTokenDto?> GetPasswordResetAsync(string resetToken, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<PasswordResetTokenDto>(new CommandDefinition(
            commandText: "dbo.usp_Identity_GetPasswordReset",
            parameters: new { ResetToken = resetToken },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ConfirmPasswordResetAsync(string resetToken, string newPasswordHash, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            commandText: "dbo.usp_Identity_ConfirmPasswordReset",
            parameters: new { ResetToken = resetToken, NewPasswordHash = newPasswordHash },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<UserProfileDto>(new CommandDefinition(
            commandText: "dbo.usp_Identity_GetMyProfile",
            parameters: new { UserId = userId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }
}
