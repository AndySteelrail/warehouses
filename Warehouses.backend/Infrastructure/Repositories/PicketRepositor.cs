using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Репозиторий для работы с пикетами
/// </summary>
public class PicketRepository : Repository<Picket>, IPicketRepository
{
    public PicketRepository(AppDbContext context) : base(context) { }

    public new async Task<IEnumerable<Picket>> GetAllAsync() =>
        await _context.Pickets.OrderBy(p => p.Name).ToListAsync();

    public async Task<IEnumerable<Picket>> GetByWarehouseIdAsync(int warehouseId) =>
        await _context.Pickets.Where(p => p.WarehouseId == warehouseId).OrderBy(p => p.Name).ToListAsync();

    public async Task<IEnumerable<Picket>> GetByWarehouseIdAtTimeAsync(int warehouseId, DateTime time) =>
        await _context.Pickets
            .Where(p => p.WarehouseId == warehouseId && 
                       p.CreatedAt <= time && 
                       (p.ClosedAt == null || p.ClosedAt > time))
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Picket?> GetByNameAsync(int warehouseId, string name)
    {
        return await _context.Pickets
            .FirstOrDefaultAsync(p => 
                p.WarehouseId == warehouseId && 
                p.Name == name &&
                p.ClosedAt == null);
    }

    public async Task<Picket?> GetByNameAtTimeAsync(int warehouseId, string name, DateTime time)
    {
        return await _context.Pickets
            .FirstOrDefaultAsync(p => 
                p.WarehouseId == warehouseId && 
                p.Name == name &&
                p.CreatedAt <= time && 
                (p.ClosedAt == null || p.ClosedAt > time));
    }

    public async Task<Picket?> GetByNameForUniquenessCheckAsync(int warehouseId, string name, DateTime creationTime)
    {
        return await _context.Pickets
            .FirstOrDefaultAsync(p => 
                p.WarehouseId == warehouseId && 
                p.Name == name &&
                // Проверяем, что пикет был активен в момент создания нового пикета
                p.CreatedAt <= creationTime && 
                (p.ClosedAt == null || p.ClosedAt > creationTime));
    }

    public async Task<bool> IsPicketInUseAsync(int picketId, DateTime? date = null)
    {
        date ??= DateTime.UtcNow;
        
        return await _context.PlatformPickets
            .Include(pp => pp.Platform)
            .AnyAsync(pp => pp.PicketId == picketId && 
                            pp.AssignedAt <= date && 
                            (pp.UnassignedAt == null || pp.UnassignedAt > date) &&
                            pp.Platform.CreatedAt <= date && 
                            (pp.Platform.ClosedAt == null || pp.Platform.ClosedAt > date));
    }
    
    public async Task<Picket?> GetByNameAndWarehouseAsync(string name, int warehouseId)
    {
        return await _context.Pickets
            .FirstOrDefaultAsync(p => 
                p.Name == name && 
                p.WarehouseId == warehouseId &&
                p.ClosedAt == null);
    }

    public async Task<IEnumerable<Picket>> GetByPlatformIdAsync(int platformId)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && pp.UnassignedAt == null)
            .Include(pp => pp.Picket)
            .OrderBy(pp => pp.Picket.Name)
            .Select(pp => pp.Picket)
            .ToListAsync();
    }

    public async Task<IEnumerable<Picket>> GetByPlatformIdAtTimeAsync(int platformId, DateTime time)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && 
                        pp.AssignedAt <= time && 
                        (pp.UnassignedAt == null || pp.UnassignedAt > time))
            .Include(pp => pp.Picket)
            .OrderBy(pp => pp.Picket.Name)
            .Select(pp => pp.Picket)
            .ToListAsync();
    }
}