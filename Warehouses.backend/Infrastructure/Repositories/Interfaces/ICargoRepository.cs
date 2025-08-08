using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с грузами
/// </summary>
public interface ICargoRepository : IRepository<Cargo>
{
    Task<IEnumerable<Cargo>> GetByPlatformIdAsync(int platformId);
    Task<IEnumerable<Cargo>> GetGoodHistoryAsync(int platformId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Cargo?> GetLatestCargoRecordAsync(int platformId, DateTime? date = null);
    Task<Cargo?> GetCargoRecordAtTimeAsync(int platformId, DateTime time);
    Task<decimal> GetRemainderAtTimeAsync(int platformId, DateTime time);
    Task<Cargo?> GetCargoRecordAtExactTimeAsync(int platformId, DateTime time);
}