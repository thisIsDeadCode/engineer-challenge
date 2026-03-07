namespace I_am_engineer.Identity.Application.Responses;

public sealed record AuthTokensResponse : BaseResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }

    public AuthTokensResponse(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        DateTimeOffset refreshTokenExpiresAt,
        bool isSuccess,
        string? message)
        : base(isSuccess, message)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
    }
}
