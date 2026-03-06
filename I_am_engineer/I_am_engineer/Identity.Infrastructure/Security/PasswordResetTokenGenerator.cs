using System.Security.Cryptography;
using I_am_engineer.Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace I_am_engineer.Identity.Infrastructure.Security;

public sealed class PasswordResetTokenGenerator : IPasswordResetTokenGenerator
{
    private readonly TimeSpan _tokenLifetime;

    public PasswordResetTokenGenerator(IConfiguration configuration)
    {
        var lifetimeInMinutes = int.TryParse(configuration["Auth:PasswordReset:TokenLifetimeMinutes"], out var configured)
            ? configured
            : 30;

        _tokenLifetime = TimeSpan.FromMinutes(lifetimeInMinutes);
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(48);
        var token = Base64UrlEncode(tokenBytes);
        var expiresAt = DateTimeOffset.UtcNow.Add(_tokenLifetime);

        return (token, expiresAt);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
