using Warehouses.backend.Models;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса для управления грузами
/// </summary>
public interface ICargoService
{
    Task RecordGoodOperationAsync(
        int platformId, 
        int goodTypeId, 
        decimal? coming = null, 
        decimal? consumption = null,
        DateTime? recordedAt = null
        );
    Task<Cargo> GetCurrentGoodAsync(int platformId, DateTime? asOfDate = null);
}