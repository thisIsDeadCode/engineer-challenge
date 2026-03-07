using I_am_engineer.Identity.Application.DTOs.RateLimiter;
using I_am_engineer.Identity.Application.Responses;
using MediatR;

namespace I_am_engineer.Identity.Application.Abstractions;

public interface IRateLimitedRequest<out TResponse> : IRequest<TResponse>
    where TResponse : BaseResponse
{
    string RateLimitKey { get; }

    int MaxAttempts { get; }

    TimeSpan Window { get; }

    TResponse CreateRateLimitExceededResponse(RateLimitDecision decision);
}
