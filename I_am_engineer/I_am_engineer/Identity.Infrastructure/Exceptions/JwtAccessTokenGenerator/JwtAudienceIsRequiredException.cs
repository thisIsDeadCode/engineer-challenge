namespace I_am_engineer.Identity.Infrastructure.Exceptions.JwtAccessTokenGenerator;

public sealed class JwtAudienceIsRequiredException : Exception
{
    public JwtAudienceIsRequiredException()
        : base("JWT audience cannot be null, empty, or whitespace.")
    {
    }
}
