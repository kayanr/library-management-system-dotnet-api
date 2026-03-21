namespace LibraryManagement.Api.Services;

public class ServiceResult<T>
{
    public bool Success { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ServiceErrorType? ErrorType { get; private set; }

    public static ServiceResult<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public static ServiceResult<T> Fail(string message, ServiceErrorType errorType) =>
        new() { Success = false, ErrorMessage = message, ErrorType = errorType };
}

public enum ServiceErrorType
{
    NotFound,
    Conflict,
    BusinessRule,
    Validation
}
