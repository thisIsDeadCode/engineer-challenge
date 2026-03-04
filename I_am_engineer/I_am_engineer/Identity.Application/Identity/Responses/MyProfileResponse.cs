namespace I_am_engineer.Identity.Application.Identity.Responses;

public sealed record MyProfileResponse(Guid UserId, string Email, string DisplayName);
