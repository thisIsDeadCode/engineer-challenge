using I_am_engineer.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Identity.Tests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            ReplaceWithMockedUserRepository(services);
        });
    }

    private static void ReplaceWithMockedUserRepository(IServiceCollection services)
    {
        var userRepositoryDescriptor = services.FirstOrDefault(service => service.ServiceType == typeof(IUserRepository));
        if (userRepositoryDescriptor is not null)
        {
            services.Remove(userRepositoryDescriptor);
        }

        var userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
        services.AddSingleton(userRepositoryMock.Object);
    }
}
