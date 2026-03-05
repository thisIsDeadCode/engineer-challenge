namespace I_am_engineer.Identity.Infrastructure.Exceptions;

public sealed class JwtSecretIsRequiredException : Exception
{
    public JwtSecretIsRequiredException()
        : base("JWT secret cannot be null, empty, or whitespace.")
    {
    }
}
