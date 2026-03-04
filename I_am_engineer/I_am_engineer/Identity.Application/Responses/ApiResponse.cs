namespace I_am_engineer.Identity.Application.Responses;

public sealed record ApiResponse<T>(T? Data, string Message)
{
    public static ApiResponse<T> Success(T? data, string message = "Success") => new(data, message);
}
