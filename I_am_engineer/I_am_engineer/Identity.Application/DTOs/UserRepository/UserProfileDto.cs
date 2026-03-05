namespace I_am_engineer.Identity.Application.DTOs.UserRepository;

public sealed record UserProfileDto(Guid UserId, string Email, string DisplayName);
