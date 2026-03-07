using I_am_engineer.Identity.Infrastructure.Exceptions;
namespace I_am_engineer.Identity.Infrastructure.Exceptions.JwtAccessTokenGenerator;

public sealed class JwtAudienceIsRequiredException : InfrastructureException
{
    public JwtAudienceIsRequiredException()
        : base("JWT audience cannot be null, empty, or whitespace.")
    {
    }
}
