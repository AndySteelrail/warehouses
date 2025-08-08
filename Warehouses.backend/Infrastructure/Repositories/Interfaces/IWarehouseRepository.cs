using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Интерфейс репозитория для работы со складами
/// </summary>
public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByNameAsync(string name);
    Task<IEnumerable<Warehouse>> GetWarehousesOnlyAsync();
    Task<IEnumerable<Warehouse>> GetWarehousesAtTimeAsync(DateTime time);
    Task<IEnumerable<Warehouse>> GetAllWithChildrenAsync();
}