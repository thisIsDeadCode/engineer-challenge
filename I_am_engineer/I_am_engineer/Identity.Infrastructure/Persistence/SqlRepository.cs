using System.Data;
using Dapper;
using I_am_engineer.Identity.Infrastructure.Exceptions.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Persistence;

public abstract class SqlRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("IdentityDb")
        ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured.");

    protected async Task<T> ExecuteReadAsync<T>(Func<SqlConnection, Task<T>> action, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await action(connection);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> action, CancellationToken cancellationToken)
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

    protected static async Task EnsureProcedureSucceededAsync(
        SqlConnection connection,
        string procedureName,
        object? parameters,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var succeeded = await connection.QuerySingleOrDefaultAsync<bool?>(new CommandDefinition(
            commandText: procedureName,
            parameters: parameters,
            commandType: CommandType.StoredProcedure,
            transaction: transaction,
            cancellationToken: cancellationToken));

        if (succeeded is not true)
        {
            throw new StoredProcedureUnexpectedResultException(procedureName);
        }
    }
}
