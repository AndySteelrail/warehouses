using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса для управления площадками
/// </summary>
public interface IPlatformService
{
    Task<Platform> CreatePlatformAsync(int warehouseId, string name, DateTime? createdAt = null);
    Task<Platform> CreatePlatformAsync(int warehouseId, string name, IEnumerable<int> picketIds, DateTime? createdAt = null);
    Task<Platform> GetPlatformAsync(int id);
    Task<IEnumerable<Platform>> GetAllPlatformsAsync();
    Task UpdatePlatformAsync(int id, string name);

    Task DeletePlatformAsync(int id);
    Task<IEnumerable<Platform>> GetPlatformsByWarehouseAsync(int warehouseId);
    Task<IEnumerable<Platform>> GetPlatformsByWarehouseAtTimeAsync(int warehouseId, DateTime time);
    Task<IEnumerable<int>> GetPicketIdsByPlatformAtTimeAsync(int platformId, DateTime time);
    Task AddPicketsToPlatformAsync(int platformId, IEnumerable<int> picketIds);
    Task RemovePicketsFromPlatformAsync(int platformId, IEnumerable<int> picketIds);
}