using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса для управления площадками
/// </summary>
public interface IPlatformService
{
    Task<Platform> CreatePlatformAsync(int warehouseId, string name, IEnumerable<int> picketIds, DateTime? createdAt = null);
    Task<Platform> GetPlatformAsync(int id);
    Task<IEnumerable<Platform>> GetAllPlatformsAsync();
    Task UpdatePlatformAsync(int id, string name);
}