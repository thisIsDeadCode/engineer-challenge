using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.Exceptions.PasswordHasher;
using Microsoft.AspNetCore.Identity;

namespace I_am_engineer.Identity.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<string> Hasher = new();

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new PasswordIsRequiredException();
        }

        return Hasher.HashPassword(password, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new PasswordIsRequiredException();
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new PasswordHashIsRequiredException();
        }

        var verificationResult = Hasher.VerifyHashedPassword(password, passwordHash, password);

        return verificationResult is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
