using Warehouses.backend.DTO.Tree;
using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса для управления складами и получения всего дерева складской иерархии
/// </summary>
public interface IWarehouseService
{
    Task<Warehouse> CreateWarehouseAsync(string name, DateTime? createdAt = null);

    Task<IEnumerable<Warehouse>> GetAllWarehousesAsync();
    Task<IEnumerable<Warehouse>> GetWarehousesAtTimeAsync(DateTime time);
    Task<WarehousesTreeDTO> GetWarehousesTreeAsync(DateTime time, int? cargoTypeId = null);

    Task CloseWarehouseAsync(int id, DateTime? closedAt = null);
    Task UpdateWarehouseAsync(int id, string name);
}