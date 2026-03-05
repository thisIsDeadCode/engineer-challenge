namespace I_am_engineer.Identity.Application.Abstractions;

public interface ITokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(Guid userId);

    string GenerateRefreshToken();
}
