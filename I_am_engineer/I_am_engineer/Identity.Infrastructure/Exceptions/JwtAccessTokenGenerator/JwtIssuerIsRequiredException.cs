using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.JwtAccessTokenGenerator;

public sealed class JwtIssuerIsRequiredException : InfrastructureException
{
    public JwtIssuerIsRequiredException()
        : base("JWT issuer cannot be null, empty, or whitespace.")
    {
    }
}
