using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;
using Warehouses.backend.Repositories.Interfaces;
using Warehouses.backend.Services;

namespace Warehouses.backend.App.Services;

/// <summary>
/// Сервис для управления площадками
/// </summary>
public class PlatformService : IPlatformService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly IPicketRepository _picketRepository;
    private readonly IPlatformPicketRepository _platformPicketRepository;
    private readonly ILogger<PlatformService> _logger;

    public PlatformService(
        IWarehouseRepository warehouseRepository,
        IPlatformRepository platformRepository,
        IPicketRepository picketRepository,
        IPlatformPicketRepository platformPicketRepository,
        ILogger<PlatformService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _platformRepository = platformRepository;
        _picketRepository = picketRepository;
        _platformPicketRepository = platformPicketRepository;
        _logger = logger;
    }
    
    public async Task<Platform> CreatePlatformAsync(int warehouseId, string name, IEnumerable<int> picketIds, DateTime? createdAt = null)
    {
        try
        {
            // Проверяем существование склада
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                throw new NotFoundException($"Склад с id {warehouseId} не найден");
            
            // Проверяем, есть ли пикеты в складе
            var warehousePickets = await _picketRepository.GetByWarehouseIdAsync(warehouseId);
            if (!warehousePickets.Any())
                throw new InvalidOperationException("Невозможно создать площадку для склада без пикетов");
            
            // Проверяем, что все запрошенные пикеты принадлежат складу
            var invalidPickets = picketIds.Except(warehousePickets.Select(p => p.Id));
            if (invalidPickets.Any())
                throw new InvalidOperationException(
                    $"Пикеты [{string.Join(", ", invalidPickets)}] не принадлежат складу");
            
            // Проверяем уникальность имени площадки в рамках склада
            var existing = await _platformRepository.GetByNameAsync(warehouseId, name);
            if (existing != null)
                throw new InvalidOperationException($"Площадка с именем '{name}' уже существует на складе");
            
            // Проверяем, что пикеты свободны
            await ValidatePicketsAvailabilityAsync(picketIds);
            
            // Создаем площадку
            var platform = new Platform
            {
                Name = name,
                WarehouseId = warehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _platformRepository.AddAsync(platform);
            await _platformRepository.SaveChangesAsync();
            
            // Добавляем пикеты к площадке
            await _platformPicketRepository.AddPicketsToPlatformAsync(platform.Id, picketIds, createdAt);
            
            return platform;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания площадки");
            throw new ApplicationException("Ошибка создания площадки", ex);
        }
    }

    private async Task ValidatePicketsAvailabilityAsync(IEnumerable<int> picketIds)
    {
        foreach (var picketId in picketIds)
        {
            if (await _picketRepository.IsPicketInUseAsync(picketId))
            {
                var picket = await _picketRepository.GetByIdAsync(picketId);
                throw new InvalidOperationException(
                    $"Пикет '{picket?.Name}' уже назначен на другую активную площадку");
            }
        }
    }

    public async Task<Platform> GetPlatformAsync(int id)
    {
        var platform = await _platformRepository.GetByIdAsync(id);
        if (platform == null)
            throw new NotFoundException($"Площадка с id {id} не найдена");
        
        return platform;
    }

    public async Task<IEnumerable<Platform>> GetAllPlatformsAsync()
    {
        return await _platformRepository.GetAllAsync();
    }

    public async Task UpdatePlatformAsync(int id, string name)
    {
        var platform = await _platformRepository.GetByIdAsync(id);
        if (platform == null)
            throw new NotFoundException($"Площадка с id {id} не найдена");
        
        if (platform.ClosedAt.HasValue)
            throw new InvalidOperationException("Невозможно обновить закрытую площадку");
        
        // Проверяем уникальность имени
        if (!string.Equals(platform.Name, name))
        {
            var existing = await _platformRepository.GetByNameAsync(platform.WarehouseId, name);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Площадка с именем '{name}' уже существует на складе");
        }
        
        // Обновляем
        platform.Name = name;
        await _platformRepository.UpdateAsync(platform);
        await _platformRepository.SaveChangesAsync();
    }
}