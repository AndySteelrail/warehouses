using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;

namespace Warehouses.backend.Services;

/// <summary>
/// Репозиторий для работы со складами
/// </summary>
public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context) { }

    public async Task<Warehouse?> GetByNameAsync(string name) => 
        await _context.Warehouses.FirstOrDefaultAsync(w => w.Name == name && w.ClosedAt == null);

    public async Task<IEnumerable<Warehouse>> GetWarehousesOnlyAsync()
    {
        return await _context.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetWarehousesAtTimeAsync(DateTime time)
    {
        return await _context.Warehouses
            .Where(w => w.CreatedAt <= time && (w.ClosedAt == null || w.ClosedAt > time))
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetAllWithChildrenAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Pickets)
            .Include(w => w.Platforms)
            .ThenInclude(p => p.PlatformPickets)
            .ThenInclude(pp => pp.Picket)
            .ToListAsync();
    }
}