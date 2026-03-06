namespace I_am_engineer.Identity.Application.Abstractions;

public interface IPasswordResetTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken();
}
