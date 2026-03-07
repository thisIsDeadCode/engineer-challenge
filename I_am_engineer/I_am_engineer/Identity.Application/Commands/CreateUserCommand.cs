using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Application.Responses;

namespace I_am_engineer.Identity.Application.Commands;

public sealed record CreateUserCommand(string Email, string Password, string ConfirmPassword)
    : IRateLimitedRequest<AuthTokensResponse>
{
    public string RateLimitKey => "create-user";

    public int MaxAttempts => 10;

    public TimeSpan Window => TimeSpan.FromMinutes(1);

    public AuthTokensResponse CreateRateLimitExceededResponse(RateLimitDecision decision)
    {
        var retryAfter = decision.RetryAfter?.TotalSeconds ?? 0;
        return new AuthTokensResponse(
            string.Empty,
            string.Empty,
            DateTimeOffset.MinValue,
            DateTimeOffset.MinValue,
            false,
            $"Rate limit exceeded. Retry after {Math.Ceiling(retryAfter)} seconds.");
    }
}
