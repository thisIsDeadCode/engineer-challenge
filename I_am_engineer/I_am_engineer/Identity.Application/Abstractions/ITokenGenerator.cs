using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface ITokenGenerator
{
    AccessToken GenerateAccessToken(Guid userId);

    RefreshToken GenerateRefreshToken(Guid userId);
}
