using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса для управления пикетами
/// </summary>
public interface IPicketService
{
    Task<Picket> CreatePicketWithTransactionAsync(int? platformId, int? warehouseId, string picketName, string? newPlatformName = null, DateTime? createdAt = null);
    Task UpdatePicketAsync(int id, string name);
    Task ClosePicketAsync(int id, DateTime? closedAt = null);
    Task<IEnumerable<Picket>> GetPicketsByWarehouseAsync(int warehouseId);
    Task<IEnumerable<Picket>> GetPicketsByWarehouseAtTimeAsync(int warehouseId, DateTime time);
    Task<IEnumerable<Picket>> GetPicketsByPlatformAsync(int platformId);
    Task<IEnumerable<Picket>> GetPicketsByPlatformAtTimeAsync(int platformId, DateTime time);
} 