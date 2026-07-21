namespace HR.Common.Results;

/// <summary>
/// Generic result wrapper for operations.
/// </summary>
public sealed record Result<TData>(bool IsSuccess, TData? Data, string? Error = null)
{
    public static Result<TData> Success(TData data) => new(true, data);
    public static Result<TData> Failure(string error) => new(false, default, error);
}

/// <summary>
/// Non-generic result wrapper for operations without data.
/// </summary>
public sealed record Result(bool IsSuccess, string? Error = null)
{
    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// API response wrapper for HTTP responses.
/// </summary>
public record ApiResponse<TData>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TData? Data { get; set; }
    public ErrorDetails? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<TData> Ok(TData data, string message = "Operation completed successfully.")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<TData> Created(TData data, string message = "Resource created successfully.")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<TData> Fail(string message, string? errorCode = null, object? errorDetails = null)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = errorCode, Details = errorDetails }
        };

    public static ApiResponse<TData> NotFound(string message)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = "NOT_FOUND" }
        };

    public static ApiResponse<TData> BadRequest(string message, IDictionary<string, string[]>? validationErrors = null)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails 
            { 
                Code = "VALIDATION_ERROR", 
                Details = validationErrors 
            }
        };

    public static ApiResponse<TData> Unauthorized(string message = "Unauthorized access.")
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = "UNAUTHORIZED" }
        };

    public static ApiResponse<TData> Forbidden(string message = "Access forbidden.")
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = "FORBIDDEN" }
        };
}

/// <summary>
/// Non-generic API response wrapper.
/// </summary>
public record ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ErrorDetails? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string message = "Operation completed successfully.")
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, string? errorCode = null, object? errorDetails = null)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = errorCode, Details = errorDetails }
        };

    public static ApiResponse BadRequest(string message)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails { Code = "VALIDATION_ERROR" }
        };
}

/// <summary>
/// Error details in API response.
/// </summary>
public record ErrorDetails
{
    public string? Code { get; set; }
    public object? Details { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>
/// Paginated result wrapper.
/// </summary>
public record PaginatedResult<TData>
{
    public List<TData> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<TData> Create(List<TData> items, int pageNumber, int pageSize, int totalCount)
        => new()
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

    public static PaginatedResult<TData> Empty(int pageNumber, int pageSize)
        => new()
        {
            Items = [],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
}
