using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Application.Responses;

namespace I_am_engineer.Identity.Application.Commands;

public sealed record LogoutCommand(Guid SessionId) : IRateLimitedRequest<BaseResponse>
{
    public string RateLimitKey => "logout";

    public int MaxAttempts => 10;

    public TimeSpan Window => TimeSpan.FromMinutes(1);

    public BaseResponse CreateRateLimitExceededResponse(RateLimitDecision decision)
    {
        var retryAfter = decision.RetryAfter?.TotalSeconds ?? 0;
        return new BaseResponse(false, $"Rate limit exceeded. Retry after {Math.Ceiling(retryAfter)} seconds.");
    }
}
