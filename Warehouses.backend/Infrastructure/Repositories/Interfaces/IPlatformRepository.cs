using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс репозитория для работы с площадками
/// </summary>
public interface IPlatformRepository : IRepository<Platform>
{
    Task<IEnumerable<Platform>> GetActivePlatformsAsync(DateTime date);
    Task<IEnumerable<Platform>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<Platform>> GetByWarehouseIdAtTimeAsync(int warehouseId, DateTime time);
    Task<Platform?> GetByNameAsync(int warehouseId, string name);
    Task ClosePlatformPicketsAsync(int platformId, DateTime closeTime);
}