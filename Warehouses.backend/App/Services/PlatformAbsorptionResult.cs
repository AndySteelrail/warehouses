namespace Warehouses.backend.Services;

/// <summary>
/// Вспомогательный класс для сервиса валидации
/// </summary>
public class PlatformAbsorptionResult
{
    public List<int> FullyAbsorbedPlatforms { get; set; } = new();
    public List<int> PartiallyAbsorbedPlatforms { get; set; } = new();
} 