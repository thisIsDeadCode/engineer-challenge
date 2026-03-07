using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Behaviors;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Application.Responses;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Identity.Tests;

public sealed class RateLimitBehaviorTests
{
    [Fact]
    public async Task Handle_BlocksRequest_WhenRateLimitExceeded()
    {
        var rateLimiter = new Mock<IRateLimiter>();
        rateLimiter
            .Setup(limiter => limiter.Allow(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .Returns(new RateLimitDecision(false, 0, TimeSpan.FromSeconds(10), 3, TimeSpan.FromMinutes(1)));

        var httpContextAccessor = BuildHttpContextAccessor("127.0.0.1");
        var behavior = new RateLimitBehavior<RequestPasswordResetCommand, BaseResponse>(rateLimiter.Object, httpContextAccessor);
        var request = new RequestPasswordResetCommand("user@example.com");

        var response = await behavior.Handle(request, () => Task.FromResult(new BaseResponse(true, null)), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Contains("Rate limit exceeded", response.Message);
    }

    [Fact]
    public async Task Handle_AllowsRequest_WhenRateLimitAllows()
    {
        var rateLimiter = new Mock<IRateLimiter>();
        rateLimiter
            .Setup(limiter => limiter.Allow(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .Returns(new RateLimitDecision(true, 2, null, 3, TimeSpan.FromMinutes(1)));

        var httpContextAccessor = BuildHttpContextAccessor("127.0.0.1");
        var behavior = new RateLimitBehavior<RequestPasswordResetCommand, BaseResponse>(rateLimiter.Object, httpContextAccessor);
        var request = new RequestPasswordResetCommand("user@example.com");

        var response = await behavior.Handle(request, () => Task.FromResult(new BaseResponse(true, null)), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Handle_UsesIpAndDeviceIdInEffectiveKey()
    {
        var capturedKey = string.Empty;
        var rateLimiter = new Mock<IRateLimiter>();
        rateLimiter
            .Setup(limiter => limiter.Allow(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .Callback<string, int, TimeSpan>((key, _, _) => capturedKey = key)
            .Returns(new RateLimitDecision(true, 9, null, 10, TimeSpan.FromMinutes(1)));

        var httpContextAccessor = BuildHttpContextAccessor("10.10.10.10");
        httpContextAccessor.HttpContext!.Request.Headers["X-Device-Id"] = "device-42";

        var behavior = new RateLimitBehavior<LoginCommand, AuthTokensResponse>(rateLimiter.Object, httpContextAccessor);
        var request = new LoginCommand("user@example.com", "Secret123!");

        _ = await behavior.Handle(
            request,
            () => Task.FromResult(new AuthTokensResponse("a", "r", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, true, null)),
            CancellationToken.None);

        Assert.Equal("login:user@example.com:ip:10.10.10.10:device:device-42", capturedKey);
    }

    [Fact]
    public async Task Handle_UsesForwardedIp_WhenPresent()
    {
        var capturedKey = string.Empty;
        var rateLimiter = new Mock<IRateLimiter>();
        rateLimiter
            .Setup(limiter => limiter.Allow(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .Callback<string, int, TimeSpan>((key, _, _) => capturedKey = key)
            .Returns(new RateLimitDecision(true, 9, null, 10, TimeSpan.FromMinutes(1)));

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.10.10.10");
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.5, 70.41.3.18";
        context.Request.Headers["X-Device-Id"] = "device-42";

        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var behavior = new RateLimitBehavior<LoginCommand, AuthTokensResponse>(rateLimiter.Object, httpContextAccessor);
        var request = new LoginCommand("user@example.com", "Secret123!");

        _ = await behavior.Handle(
            request,
            () => Task.FromResult(new AuthTokensResponse("a", "r", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, true, null)),
            CancellationToken.None);

        Assert.Equal("login:user@example.com:ip:203.0.113.5:device:device-42", capturedKey);
    }

    private static IHttpContextAccessor BuildHttpContextAccessor(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        return new HttpContextAccessor { HttpContext = context };
    }
}
