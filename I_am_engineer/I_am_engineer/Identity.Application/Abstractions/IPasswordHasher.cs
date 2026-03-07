using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IPasswordHasher
{
    PasswordHash Hash(string password);

    bool Verify(string password, string passwordHash);
}
