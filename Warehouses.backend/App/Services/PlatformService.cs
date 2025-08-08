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
        ICargoRepository cargoRepository,
        ILogger<PlatformService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _platformRepository = platformRepository;
        _picketRepository = picketRepository;
        _platformPicketRepository = platformPicketRepository;
        _logger = logger;
    }

    public async Task<Platform> CreatePlatformAsync(int warehouseId, string name, DateTime? createdAt = null)
    {
        try
        {
            _logger.LogInformation("Начинаем создание площадки без пикетов: WarehouseId={WarehouseId}, Name={Name}", warehouseId, name);
            
            // Проверяем существование склада
            _logger.LogInformation("Проверяем существование склада: WarehouseId={WarehouseId}", warehouseId);
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                _logger.LogError("Склад не найден: WarehouseId={WarehouseId}", warehouseId);
                throw new NotFoundException($"Warehouse with id {warehouseId} not found");
            }
            
            _logger.LogInformation("Склад найден: WarehouseId={WarehouseId}, Name={Name}", warehouse.Id, warehouse.Name);
            
            // Проверяем уникальность имени площадки в рамках склада
            _logger.LogInformation("Проверяем уникальность имени площадки: Name={Name}, WarehouseId={WarehouseId}", name, warehouseId);
            var existing = await _platformRepository.GetByNameAsync(warehouseId, name);
            if (existing != null)
            {
                _logger.LogError("Площадка с таким именем уже существует: Name={Name}, WarehouseId={WarehouseId}, ExistingPlatformId={ExistingPlatformId}", 
                    name, warehouseId, existing.Id);
                throw new InvalidOperationException($"Platform with name '{name}' already exists in warehouse");
            }
            
            // Создаем площадку
            _logger.LogInformation("Создаем площадку в базе данных: Name={Name}, WarehouseId={WarehouseId}", name, warehouseId);
            var platform = new Platform
            {
                Name = name,
                WarehouseId = warehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _platformRepository.AddAsync(platform);
            await _platformRepository.SaveChangesAsync();
            
            _logger.LogInformation("Площадка успешно создана: PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                platform.Id, platform.Name, platform.WarehouseId);
            
            return platform;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки: WarehouseId={WarehouseId}, Name={Name}", warehouseId, name);
            throw new ApplicationException("Failed to create platform", ex);
        }
    }

    public async Task<Platform> CreatePlatformAsync(int warehouseId, string name, IEnumerable<int> picketIds, DateTime? createdAt = null)
    {
        try
        {
            // Проверяем существование склада
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                throw new NotFoundException($"Warehouse with id {warehouseId} not found");
            
            // Проверяем, есть ли пикеты в складе
            var warehousePickets = await _picketRepository.GetByWarehouseIdAsync(warehouseId);
            if (!warehousePickets.Any())
                throw new InvalidOperationException("Cannot create platform for warehouse without pickets");
            
            // Проверяем, что все запрошенные пикеты принадлежат складу
            var invalidPickets = picketIds.Except(warehousePickets.Select(p => p.Id));
            if (invalidPickets.Any())
                throw new InvalidOperationException(
                    $"Pickets [{string.Join(", ", invalidPickets)}] do not belong to warehouse");
            
            // Проверяем уникальность имени площадки в рамках склада
            var existing = await _platformRepository.GetByNameAsync(warehouseId, name);
            if (existing != null)
                throw new InvalidOperationException($"Platform with name '{name}' already exists in warehouse");
            
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
                    $"Picket '{picket?.Name}' is already assigned to another active platform");
            }
        }
    }

    public async Task<Platform> GetPlatformAsync(int id)
    {
        var platform = await _platformRepository.GetByIdAsync(id);
        if (platform == null)
            throw new NotFoundException($"Platform with id {id} not found");
        
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
            throw new NotFoundException($"Platform with id {id} not found");
        
        if (platform.ClosedAt.HasValue)
            throw new InvalidOperationException("Cannot update closed platform");
        
        // Проверяем уникальность имени
        if (!string.Equals(platform.Name, name))
        {
            var existing = await _platformRepository.GetByNameAsync(platform.WarehouseId, name);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Platform with name '{name}' already exists in warehouse");
        }
        
        // Обновляем
        platform.Name = name;
        await _platformRepository.UpdateAsync(platform);
        await _platformRepository.SaveChangesAsync();
    }
}