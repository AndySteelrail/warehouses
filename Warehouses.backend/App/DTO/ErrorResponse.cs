namespace Warehouses.backend.DTO;

/// <summary>
/// Стандартный формат ошибки API
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ErrorCode { get; set; }
    
    public ErrorResponse(string message, string? details = null, string? errorCode = null)
    {
        Message = message;
        Details = details;
        ErrorCode = errorCode;
    }
} 