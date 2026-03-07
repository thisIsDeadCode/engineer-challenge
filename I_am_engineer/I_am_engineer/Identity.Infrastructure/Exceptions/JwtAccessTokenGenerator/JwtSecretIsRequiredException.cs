using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.JwtAccessTokenGenerator;

public sealed class JwtSecretIsRequiredException : InfrastructureException
{
    public JwtSecretIsRequiredException()
        : base("JWT secret cannot be null, empty, or whitespace.")
    {
    }
}
