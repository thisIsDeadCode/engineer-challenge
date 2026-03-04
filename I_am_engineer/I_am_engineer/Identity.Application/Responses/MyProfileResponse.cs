namespace I_am_engineer.Identity.Application.Responses;

public sealed record MyProfileResponse : BaseResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }


    public MyProfileResponse(Guid userId, string email, string displayName, bool isSuccess, string? message)
        : base(isSuccess, message)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
    }
}