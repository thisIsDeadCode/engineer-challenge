namespace I_am_engineer.Identity.Application.Responses;

public sealed record MyProfileResponse(Guid UserId, string Email, string DisplayName);
