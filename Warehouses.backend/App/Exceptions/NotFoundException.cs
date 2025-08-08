namespace Warehouses.backend.Exceptions;

/// <summary>
/// Исключение, возникающее когда сущность не найдена
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}