using I_am_engineer.Identity.Infrastructure.Exceptions;

namespace I_am_engineer.Identity.Infrastructure.Exceptions.Persistence;

public sealed class StoredProcedureUnexpectedResultException(string procedureName)
    : InfrastructureException($"Stored procedure '{procedureName}' returned an unexpected result.")
{
}
