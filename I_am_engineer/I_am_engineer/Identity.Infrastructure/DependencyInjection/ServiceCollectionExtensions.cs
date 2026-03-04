using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Infrastructure.Persistence;
using I_am_engineer.Identity.Infrastructure.RateLimiting;
using I_am_engineer.Identity.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace I_am_engineer.Identity.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtAccessTokenGenerator>();
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        services.AddSingleton<RefreshTokenStore>();
        services.AddSingleton<TokenRevocationStore>();

        return services;
    }
}
