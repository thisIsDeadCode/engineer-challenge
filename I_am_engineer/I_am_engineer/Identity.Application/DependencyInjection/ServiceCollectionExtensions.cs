using I_am_engineer.Identity.Application.Handlers.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using I_am_engineer.Identity.Infrastructure.Services;

namespace I_am_engineer.Identity.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly));
        services.AddSingleton<EventService>();

        return services;
    }
}
