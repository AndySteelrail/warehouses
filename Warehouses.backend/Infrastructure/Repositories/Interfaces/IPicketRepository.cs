using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с пикетами
/// </summary>
public interface IPicketRepository : IRepository<Picket>
{
    Task<IEnumerable<Picket>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<Picket>> GetByWarehouseIdAtTimeAsync(int warehouseId, DateTime time);
    Task<bool> IsPicketInUseAsync(int picketId, DateTime? date = null);
    Task<Picket?> GetByNameAsync(int warehouseId, string name);
    Task<Picket?> GetByNameAtTimeAsync(int warehouseId, string name, DateTime time);
    Task<Picket?> GetByNameForUniquenessCheckAsync(int warehouseId, string name, DateTime creationTime);
    Task<Picket?> GetByNameAndWarehouseAsync(string name, int warehouseId);
    Task<IEnumerable<Picket>> GetByPlatformIdAsync(int platformId);
    Task<IEnumerable<Picket>> GetByPlatformIdAtTimeAsync(int platformId, DateTime time);
}