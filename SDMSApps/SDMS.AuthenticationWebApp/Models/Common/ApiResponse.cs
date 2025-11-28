namespace SDMS.AuthenticationWebApp.Models.Common;

/// <summary>
/// Standard API response wrapper for consistent API responses
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string? message = null, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Non-generic API response for operations without data
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse SuccessResponse(string? message = null, string? correlationId = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse ErrorResponse(string message, List<string>? errors = null, string? correlationId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }
}

