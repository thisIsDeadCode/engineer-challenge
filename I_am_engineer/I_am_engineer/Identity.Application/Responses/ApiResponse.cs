namespace I_am_engineer.Identity.Application.Responses;

public sealed record ApiResponse<T>(T? Data, bool IsSuccess, string? Message = null)
{
    public static ApiResponse<T> Success(T? data) => new(data, true);
    public static ApiResponse<T> Error(T? data, string message) => new(data, false, message);
}

public sealed record ApiResponse(bool IsSuccess, string? Message = null)
{
    public static ApiResponse Success() => new(true);
    public static ApiResponse Error(string message) => new(false, message);
}
