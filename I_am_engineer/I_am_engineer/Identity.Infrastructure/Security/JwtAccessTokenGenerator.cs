using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.ValueObjects;
using I_am_engineer.Identity.Infrastructure.Exceptions.JwtAccessTokenGenerator;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Security;

public sealed class JwtAccessTokenGenerator : ITokenGenerator
{
    private readonly byte[] _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _accessTokenLifetime;

    public JwtAccessTokenGenerator(IConfiguration configuration)
    {
        var jwtSecret = configuration["Auth:Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new JwtSecretIsRequiredException();
        }

        var issuer = configuration["Auth:Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new JwtIssuerIsRequiredException();
        }

        var audience = configuration["Auth:Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new JwtAudienceIsRequiredException();
        }

        _secret = Encoding.UTF8.GetBytes(jwtSecret);
        _issuer = issuer;
        _audience = audience;

        var lifetimeInMinutes = int.TryParse(configuration["Auth:Jwt:AccessTokenLifetimeMinutes"], out var configured)
            ? configured
            : 15;
        _accessTokenLifetime = TimeSpan.FromMinutes(lifetimeInMinutes);
    }


    public AccessToken GenerateAccessToken(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(_accessTokenLifetime);

        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "HS256",
            typ = "JWT"
        }));

        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = userId.ToString(),
            iss = _issuer,
            aud = _audience,
            iat = now.ToUnixTimeSeconds(),
            exp = expiresAt.ToUnixTimeSeconds()
        }));

        var signature = ComputeSignature($"{header}.{payload}");

        return new AccessToken($"{header}.{payload}.{signature}",
                                    expiresAt);
    }

    public RefreshToken GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return new RefreshToken(Base64UrlEncode(bytes));
    }

    private string ComputeSignature(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        using var hmac = new HMACSHA256(_secret);
        var hash = hmac.ComputeHash(bytes);
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
