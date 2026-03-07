using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.ValueObjects;
using System.Security.Cryptography;

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

    public PasswordResetToken GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(48);
        var token = Base64UrlEncode(tokenBytes);
        var expiresAt = DateTimeOffset.UtcNow.Add(_tokenLifetime);

        return PasswordResetToken.Create(token, false, expiresAt);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
