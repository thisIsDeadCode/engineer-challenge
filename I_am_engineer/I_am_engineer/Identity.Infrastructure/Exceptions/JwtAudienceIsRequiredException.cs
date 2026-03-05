namespace I_am_engineer.Identity.Infrastructure.Exceptions;

public sealed class JwtAudienceIsRequiredException : Exception
{
    public JwtAudienceIsRequiredException()
        : base("JWT audience cannot be null, empty, or whitespace.")
    {
    }
}
