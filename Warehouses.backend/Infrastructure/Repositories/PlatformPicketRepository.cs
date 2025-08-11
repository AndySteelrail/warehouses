using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Репозиторий для работы со связями площадок и пикетов
/// </summary>
public class PlatformPicketRepository : Repository<PlatformPicket>, IPlatformPicketRepository
{
    public PlatformPicketRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PlatformPicket>> GetByPlatformIdAsync(int platformId)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && pp.UnassignedAt == null)
            .Include(pp => pp.Picket)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlatformPicket>> GetByPlatformIdAtTimeAsync(int platformId, DateTime time)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && 
                        pp.AssignedAt <= time && 
                        (pp.UnassignedAt == null || pp.UnassignedAt > time))
            .Include(pp => pp.Picket)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlatformPicket>> GetByPicketIdAsync(int picketId)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PicketId == picketId && pp.UnassignedAt == null)
            .Include(pp => pp.Platform)
            .ToListAsync();
    }

    public async Task AddPicketsToPlatformAsync(int platformId, IEnumerable<int> picketIds, DateTime? assignedAt = null)
    {
        var assignmentTime = assignedAt ?? DateTime.UtcNow;
        
        foreach (var picketId in picketIds)
        {
            await _context.PlatformPickets.AddAsync(new PlatformPicket
            {
                PlatformId = platformId,
                PicketId = picketId,
                AssignedAt = assignmentTime,
                UnassignedAt = null
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task RemovePicketsFromPlatformAsync(int platformId, IEnumerable<int> picketIds, DateTime? unassignedAt = null)
    {
        var unassignmentTime = unassignedAt ?? DateTime.UtcNow;
        
        var relations = await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && 
                        picketIds.Contains(pp.PicketId) && 
                        pp.UnassignedAt == null)
            .ToListAsync();

        foreach (var relation in relations)
        {
            relation.UnassignedAt = unassignmentTime;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<int>> GetPicketIdsByPlatformIdAsync(int platformId)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && pp.UnassignedAt == null)
            .Include(pp => pp.Picket)
            .OrderBy(pp => pp.Picket.Name)
            .Select(pp => pp.PicketId)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetPicketIdsByPlatformIdAtTimeAsync(int platformId, DateTime time)
    {
        return await _context.PlatformPickets
            .Where(pp => pp.PlatformId == platformId && 
                        pp.AssignedAt <= time && 
                        (pp.UnassignedAt == null || pp.UnassignedAt > time))
            .Include(pp => pp.Picket)
            .OrderBy(pp => pp.Picket.Name)
            .Select(pp => pp.PicketId)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetPicketIdsByWarehouseIdAsync(int warehouseId)
    {
        return await _context.Pickets
            .Where(p => p.WarehouseId == warehouseId)
            .OrderBy(p => p.Name)
            .Select(p => p.Id)
            .ToListAsync();
    }

    public async Task<Dictionary<int, List<int>>> GetPlatformPicketsMappingAsync(int warehouseId)
    {
        var platforms = await _context.Platforms
            .Where(p => p.WarehouseId == warehouseId && p.ClosedAt == null)
            .Include(p => p.PlatformPickets)
            .ThenInclude(pp => pp.Picket)
            .ToListAsync();

        var mapping = new Dictionary<int, List<int>>();
        foreach (var platform in platforms)
        {
            mapping[platform.Id] = platform.PlatformPickets
                .Where(pp => pp.UnassignedAt == null)
                .OrderBy(pp => pp.Picket.Name)
                .Select(pp => pp.PicketId)
                .ToList();
        }

        return mapping;
    }

    public async Task<Dictionary<int, List<int>>> GetPlatformPicketsMappingAtTimeAsync(int warehouseId, DateTime time)
    {
        var platforms = await _context.Platforms
            .Where(p => p.WarehouseId == warehouseId && 
                       p.CreatedAt <= time && 
                       (p.ClosedAt == null || p.ClosedAt > time))
            .Include(p => p.PlatformPickets)
            .ThenInclude(pp => pp.Picket)
            .ToListAsync();

        var mapping = new Dictionary<int, List<int>>();
        foreach (var platform in platforms)
        {
            mapping[platform.Id] = platform.PlatformPickets
                .Where(pp => pp.AssignedAt <= time && 
                           (pp.UnassignedAt == null || pp.UnassignedAt > time))
                .OrderBy(pp => pp.Picket.Name)
                .Select(pp => pp.PicketId)
                .ToList();
        }

        return mapping;
    }

    public async Task<bool> ArePicketsSequentialAsync(IEnumerable<int> picketIds, int warehouseId)
    {
        if (!picketIds.Any()) return true;
        if (picketIds.Count() == 1) return true;

        // Получаем все пикеты склада, отсортированные по названию
        var allWarehousePickets = await _context.Pickets
            .Where(p => p.WarehouseId == warehouseId)
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToListAsync();

        // Получаем выбранные пикеты, отсортированные по названию
        var selectedPickets = await _context.Pickets
            .Where(p => picketIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => p.Name )
            .ToListAsync();

        // Находим индекс первого выбранного пикета в списке пикетов склада
        var firstSelectedIndex = allWarehousePickets.IndexOf(selectedPickets[0]);
        if (firstSelectedIndex == -1) return false;

        // Проверяем, что все выбранные пикеты идут последовательно в общем списке
        for (int i = 0; i < selectedPickets.Count; i++)
        {
            if (firstSelectedIndex + i >= allWarehousePickets.Count || 
                selectedPickets[i] != allWarehousePickets[firstSelectedIndex + i])
            {
                return false;
            }
        }

        return true;
    }

    public async Task<IEnumerable<Platform>> GetPlatformsByPicketIdsAsync(IEnumerable<int> picketIds)
    {
        return await _context.Platforms
            .Where(p => p.PlatformPickets.Any(pp => picketIds.Contains(pp.PicketId) && pp.UnassignedAt == null) && p.ClosedAt == null)
            .Include(p => p.PlatformPickets)
            .ToListAsync();
    }

    public async Task<bool> ArePicketsInFutureClosedPlatformsAsync(IEnumerable<int> picketIds, DateTime time)
    {
        var result = await _context.PlatformPickets
            .Include(pp => pp.Platform)
            .AnyAsync(pp => picketIds.Contains(pp.PicketId) && 
                           pp.AssignedAt <= time && 
                           (pp.UnassignedAt == null || pp.UnassignedAt > time) &&
                           pp.Platform.ClosedAt.HasValue && 
                           pp.Platform.ClosedAt > time);
        return result;
    }
}