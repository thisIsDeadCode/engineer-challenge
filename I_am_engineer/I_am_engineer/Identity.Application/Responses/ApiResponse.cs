namespace I_am_engineer.Identity.Application.Responses;

public sealed record ApiResponse<T>(T? Data, bool isSuccess, string? Message = null)
{
    public static ApiResponse<T> Success(T? data, bool isSuccess) => new(data, true);
    public static ApiResponse<T> Error(T? data, bool isSuccess, string message) => new(data, false, message);
}
