using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Репозиторий для работы с площадками
/// </summary>
public class PlatformRepository : Repository<Platform>, IPlatformRepository
{
    public PlatformRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Platform>> GetActivePlatformsAsync(DateTime date)
    {
        return await _context.Platforms
            .Where(p => p.CreatedAt <= date && (p.ClosedAt == null || p.ClosedAt > date))
            .Include(p => p.Warehouse)
            .ToListAsync();
    }

    public async Task<IEnumerable<Platform>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.Platforms
            .Where(p => p.WarehouseId == warehouseId)
            .Include(p => p.Warehouse)
            .Include(p => p.PlatformPickets)
            .ThenInclude(pp => pp.Picket)
            .ToListAsync();
    }

    public async Task<IEnumerable<Platform>> GetByWarehouseIdAtTimeAsync(int warehouseId, DateTime time)
    {
        return await _context.Platforms
            .Where(p => p.WarehouseId == warehouseId && 
                       p.CreatedAt <= time && 
                       (p.ClosedAt == null || p.ClosedAt > time))
            .Include(p => p.Warehouse)
            .Include(p => p.PlatformPickets)
            .ThenInclude(pp => pp.Picket)
            .ToListAsync();
    }

    public async Task<Platform?> GetByNameAsync(int warehouseId, string name) => 
        await _context.Platforms
            .FirstOrDefaultAsync(p => p.WarehouseId == warehouseId && p.Name == name && p.ClosedAt == null);

    public new async Task<IEnumerable<Platform>> GetAllAsync()
    {
        return await _context.Platforms
            .Include(p => p.Warehouse)
            .ToListAsync();
    }

    public async Task ClosePlatformPicketsAsync(int platformId, DateTime closeTime)
    {
        var platformPickets = await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && pp.UnassignedAt == null)
            .ToListAsync();
        
        foreach (var platformPicket in platformPickets)
        {
            platformPicket.UnassignedAt = closeTime;
        }
        
        await _context.SaveChangesAsync();
    }
}