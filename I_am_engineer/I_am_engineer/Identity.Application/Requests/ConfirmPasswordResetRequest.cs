namespace I_am_engineer.Identity.Application.Requests;

public sealed record ConfirmPasswordResetRequest(string ResetToken, string NewPassword);
