using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы со связями площадок и пикетов
/// </summary>
public interface IPlatformPicketRepository : IRepository<PlatformPicket>
{
    Task<IEnumerable<PlatformPicket>> GetByPlatformIdAsync(int platformId);
    Task<IEnumerable<PlatformPicket>> GetByPlatformIdAtTimeAsync(int platformId, DateTime time);
    Task<IEnumerable<PlatformPicket>> GetByPicketIdAsync(int picketId);
    Task AddPicketsToPlatformAsync(int platformId, IEnumerable<int> picketIds, DateTime? assignedAt = null);
    Task RemovePicketsFromPlatformAsync(int platformId, IEnumerable<int> picketIds, DateTime? unassignedAt = null);
    
    Task<IEnumerable<int>> GetPicketIdsByPlatformIdAsync(int platformId);
    Task<IEnumerable<int>> GetPicketIdsByPlatformIdAtTimeAsync(int platformId, DateTime time);
    Task<IEnumerable<int>> GetPicketIdsByWarehouseIdAsync(int warehouseId);
    Task<Dictionary<int, List<int>>> GetPlatformPicketsMappingAsync(int warehouseId);
    Task<Dictionary<int, List<int>>> GetPlatformPicketsMappingAtTimeAsync(int warehouseId, DateTime time);
    Task<bool> ArePicketsSequentialAsync(IEnumerable<int> picketIds, int warehouseId);
    Task<IEnumerable<Platform>> GetPlatformsByPicketIdsAsync(IEnumerable<int> picketIds);
    Task<bool> ArePicketsInFutureClosedPlatformsAsync(IEnumerable<int> picketIds, DateTime time);
}