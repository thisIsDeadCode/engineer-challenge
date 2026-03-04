using I_am_engineer.Identity.Application.Handlers.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace I_am_engineer.Identity.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly));

        return services;
    }
}
