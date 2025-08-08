using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Репозиторий для работы с грузами
/// </summary>
public class CargoRepository : Repository<Cargo>, ICargoRepository
{
    public CargoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cargo>> GetByPlatformIdAsync(int platformId)
    {
        return await _context.Cargoes
            .Where(g => g.PlatformId == platformId)
            .Include(g => g.CargoType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cargo>> GetGoodHistoryAsync(int platformId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Cargoes
            .Where(g => g.PlatformId == platformId)
            .Include(g => g.CargoType)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(g => g.RecordedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(g => g.RecordedAt <= endDate.Value);

        return await query.ToListAsync();
    }

    public async Task<Cargo?> GetLatestCargoRecordAsync(int platformId, DateTime? date = null)
    {
        date ??= DateTime.UtcNow;
        
        return await _context.Cargoes
            .Where(g => g.PlatformId == platformId && g.RecordedAt <= date)
            .Include(g => g.CargoType)
            .OrderByDescending(g => g.RecordedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Cargo?> GetCargoRecordAtTimeAsync(int platformId, DateTime time)
    {
        return await _context.Cargoes
            .Where(g => g.PlatformId == platformId && g.RecordedAt <= time)
            .Include(g => g.CargoType)
            .OrderByDescending(g => g.RecordedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetRemainderAtTimeAsync(int platformId, DateTime time)
    {
        var latestRecord = await GetCargoRecordAtTimeAsync(platformId, time);
        return latestRecord?.Remainder ?? 0;
    }

    public async Task<Cargo?> GetCargoRecordAtExactTimeAsync(int platformId, DateTime time)
    {
        return await _context.Cargoes
            .Where(g => g.PlatformId == platformId && g.RecordedAt == time)
            .Include(g => g.CargoType)
            .FirstOrDefaultAsync();
    }
}