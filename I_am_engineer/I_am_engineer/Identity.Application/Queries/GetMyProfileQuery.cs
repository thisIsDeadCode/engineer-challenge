using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Application.Responses;

namespace I_am_engineer.Identity.Application.Queries;

public sealed record GetMyProfileQuery : IRateLimitedRequest<MyProfileResponse>
{
    public string RateLimitKey => "get-my-profile";

    public int MaxAttempts => 10;

    public TimeSpan Window => TimeSpan.FromMinutes(1);

    public MyProfileResponse CreateRateLimitExceededResponse(RateLimitDecision decision)
    {
        var retryAfter = decision.RetryAfter?.TotalSeconds ?? 0;
        return new MyProfileResponse(Guid.Empty, string.Empty, string.Empty, false, $"Rate limit exceeded. Retry after {Math.Ceiling(retryAfter)} seconds.");
    }
}
