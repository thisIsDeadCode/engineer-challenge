using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IPasswordResetTokenGenerator
{
    PasswordResetToken GenerateToken();
}
