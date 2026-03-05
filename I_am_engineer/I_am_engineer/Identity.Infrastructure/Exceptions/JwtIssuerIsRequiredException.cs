namespace I_am_engineer.Identity.Infrastructure.Exceptions;

public sealed class JwtIssuerIsRequiredException : Exception
{
    public JwtIssuerIsRequiredException()
        : base("JWT issuer cannot be null, empty, or whitespace.")
    {
    }
}
