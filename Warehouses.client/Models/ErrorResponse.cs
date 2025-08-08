namespace Warehouses.client.Models;

/// <summary>
/// Модель для обработки ошибок API
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ErrorCode { get; set; }
} 