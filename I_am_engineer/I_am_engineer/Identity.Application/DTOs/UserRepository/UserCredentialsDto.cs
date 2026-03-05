namespace I_am_engineer.Identity.Application.DTOs.UserRepository;

public sealed record UserCredentialsDto(Guid UserId, string Email, string PasswordHash, bool IsActive);
