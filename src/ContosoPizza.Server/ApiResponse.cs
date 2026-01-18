namespace ContosoPizza;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, int statusCode) =>
        Ok(data, message: string.Empty, statusCode);

    public static ApiResponse<T> Ok(T data,string message, int statusCode) =>
        new() { IsSuccess = true, Data = data, Message = message, StatusCode = statusCode };

    public static ApiResponse<T> Fail(string message, int statusCode) =>
        new() { IsSuccess = false, Message = message, StatusCode = statusCode };
}