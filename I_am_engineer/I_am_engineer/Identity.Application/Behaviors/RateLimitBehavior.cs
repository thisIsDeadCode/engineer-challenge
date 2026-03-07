using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace I_am_engineer.Identity.Application.Behaviors;

public sealed class RateLimitBehavior<TRequest, TResponse>(
    IRateLimiter rateLimiter,
    IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : BaseResponse
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRateLimitedRequest<TResponse> rateLimitedRequest)
        {
            return next();
        }

        var effectiveKey = BuildEffectiveKey(rateLimitedRequest.RateLimitKey);
        var decision = rateLimiter.Allow(effectiveKey, rateLimitedRequest.MaxAttempts, rateLimitedRequest.Window);
        if (decision.Allowed)
        {
            return next();
        }

        return Task.FromResult(rateLimitedRequest.CreateRateLimitExceededResponse(decision));
    }

    private string BuildEffectiveKey(string baseKey)
    {
        var ip = ResolveClientIp(httpContextAccessor.HttpContext);
        var deviceId = ResolveDeviceId(httpContextAccessor.HttpContext);

        var ipPart = string.IsNullOrWhiteSpace(ip) ? "unknown-ip" : ip.Trim().ToLowerInvariant();
        var devicePart = string.IsNullOrWhiteSpace(deviceId) ? "unknown-device" : deviceId.Trim().ToLowerInvariant();

        return $"{baseKey}:ip:{ipPart}:device:{devicePart}";
    }


    private static string? ResolveClientIp(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstForwardedIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstForwardedIp))
            {
                return firstForwardedIp;
            }
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? ResolveDeviceId(HttpContext? httpContext)
    {
        return httpContext?.Request.Headers["X-Device-Id"].FirstOrDefault();
    }
}
