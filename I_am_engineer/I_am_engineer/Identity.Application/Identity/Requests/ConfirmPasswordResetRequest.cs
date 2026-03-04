namespace I_am_engineer.Identity.Application.Identity.Requests;

public sealed record ConfirmPasswordResetRequest(string ResetToken, string NewPassword);
