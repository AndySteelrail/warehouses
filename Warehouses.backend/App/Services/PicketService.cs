using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;
using Warehouses.backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Warehouses.backend.Services;

/// <summary>
/// Сервис для управления пикетами
/// </summary>
public class PicketService : IPicketService
{
    private readonly IPicketRepository _picketRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly IPlatformPicketRepository _platformPicketRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<PicketService> _logger;

    public PicketService(
        IPicketRepository picketRepository,
        IPlatformRepository platformRepository,
        IPlatformPicketRepository platformPicketRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<PicketService> logger)
    {
        _picketRepository = picketRepository;
        _platformRepository = platformRepository;
        _platformPicketRepository = platformPicketRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<Picket> CreatePicketWithTransactionAsync(int? platformId, int? warehouseId, string picketName, string? newPlatformName = null, DateTime? createdAt = null)
    {
        // Начинаем транзакцию, т.к. нам нужно создать и пикет, и площадку, либо ничего из этого
        using var transaction = await _picketRepository.BeginTransactionAsync();
        
        try
        {
            Platform targetPlatform;
            
            if (platformId.HasValue)
            {
                // Создаем пикет на существующей площадке
                _logger.LogInformation("Создаем пикет на существующей площадке: PlatformId={PlatformId}", platformId.Value);
                targetPlatform = await _platformRepository.GetByIdAsync(platformId.Value);
                if (targetPlatform == null)
                {
                    _logger.LogError("Площадка не найдена: PlatformId={PlatformId}", platformId.Value);
                    throw new NotFoundException($"Platform with id {platformId.Value} not found");
                }
                
                if (targetPlatform.ClosedAt.HasValue)
                {
                    _logger.LogError("Попытка создать пикет для закрытой площадки: PlatformId={PlatformId}", platformId.Value);
                    throw new InvalidOperationException("Cannot create picket for closed platform");
                }
            }
            else
            {
                // Создаем новую площадку
                if (!warehouseId.HasValue)
                {
                    _logger.LogError("WarehouseId обязателен при создании новой площадки");
                    throw new InvalidOperationException("WarehouseId is required when creating new platform");
                }
                
                _logger.LogInformation("Создаем новую площадку: WarehouseId={WarehouseId}, Name={Name}", warehouseId.Value, newPlatformName);
                
                // Проверяем существование склада
                var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId.Value);
                if (warehouse == null)
                {
                    _logger.LogError("Склад не найден: WarehouseId={WarehouseId}", warehouseId.Value);
                    throw new NotFoundException($"Warehouse with id {warehouseId.Value} not found");
                }
                
                // Проверяем уникальность имени площадки
                var existingPlatform = await _platformRepository.GetByNameAsync(warehouseId.Value, newPlatformName ?? picketName ?? string.Empty);
                if (existingPlatform != null)
                {
                    _logger.LogError("Площадка с таким именем уже существует: Name={Name}, WarehouseId={WarehouseId}", newPlatformName, warehouseId.Value);
                    throw new InvalidOperationException($"Platform with name '{newPlatformName ?? picketName}' already exists in warehouse");
                }
                
                // Создаем новую площадку
                targetPlatform = new Platform
                {
                    Name = newPlatformName ?? picketName,
                    WarehouseId = warehouseId.Value,
                    CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
                };
                await _platformRepository.AddAsync(targetPlatform);
                await _platformRepository.SaveChangesAsync();
                
                _logger.LogInformation("Новая площадка создана: PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                    targetPlatform.Id, targetPlatform.Name, targetPlatform.WarehouseId);
            }
            
            // Проверяем уникальность имени пикета в рамках склада с учетом времени
            var recordTime = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
            _logger.LogInformation("Проверяем уникальность имени пикета: Name={Name}, WarehouseId={WarehouseId}, Time={Time}", picketName, targetPlatform.WarehouseId, recordTime);
            var existingPicket = await _picketRepository.GetByNameForUniquenessCheckAsync(targetPlatform.WarehouseId, picketName, recordTime);
            if (existingPicket != null)
            {
                _logger.LogError("Пикет с таким именем уже существует на время {Time}: Name={Name}, WarehouseId={WarehouseId}, ExistingPicketId={ExistingPicketId}, ExistingCreatedAt={ExistingCreatedAt}, ExistingClosedAt={ExistingClosedAt}", 
                    recordTime, picketName, targetPlatform.WarehouseId, existingPicket.Id, existingPicket.CreatedAt, existingPicket.ClosedAt);
                throw new InvalidOperationException($"Picket with name '{picketName}' already exists in warehouse at time {recordTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                _logger.LogInformation("Пикет с именем {Name} не найден на время {Time}, можно создавать", picketName, recordTime);
            }
            
            // Создаем пикет
            _logger.LogInformation("Создаем пикет: Name={Name}, WarehouseId={WarehouseId}", picketName, targetPlatform.WarehouseId);
            var picket = new Picket
            {
                Name = picketName,
                WarehouseId = targetPlatform.WarehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _picketRepository.AddAsync(picket);
            await _picketRepository.SaveChangesAsync();
            
            _logger.LogInformation("Пикет создан: PicketId={PicketId}, Name={Name}, WarehouseId={WarehouseId}", 
                picket.Id, picket.Name, picket.WarehouseId);
            
            // Добавляем пикет к площадке
            _logger.LogInformation("Добавляем пикет к площадке: PlatformId={PlatformId}, PicketId={PicketId}", targetPlatform.Id, picket.Id);
            await _platformPicketRepository.AddPicketsToPlatformAsync(targetPlatform.Id, new[] { picket.Id }, createdAt);
            
            // Фиксируем транзакцию
            await transaction.CommitAsync();
            
            _logger.LogInformation("Транзакция успешно завершена. Пикет создан: PicketId={PicketId}, PlatformId={PlatformId}", picket.Id, targetPlatform.Id);
            
            return picket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пикета с транзакцией. Откатываем транзакцию.");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Picket> CreatePicketAsync(int platformId, string name, DateTime? createdAt = null)
    {
        try
        {
            _logger.LogInformation("Начинаем создание пикета: PlatformId={PlatformId}, Name={Name}", platformId, name);
            
            // Проверяем существование площадки
            _logger.LogInformation("Проверяем существование площадки: PlatformId={PlatformId}", platformId);
            var platform = await _platformRepository.GetByIdAsync(platformId);
            if (platform == null)
            {
                _logger.LogError("Площадка не найдена: PlatformId={PlatformId}", platformId);
                throw new NotFoundException($"Platform with id {platformId} not found");
            }
            
            _logger.LogInformation("Площадка найдена: PlatformId={PlatformId}, WarehouseId={WarehouseId}, Name={Name}, ClosedAt={ClosedAt}", 
                platform.Id, platform.WarehouseId, platform.Name, platform.ClosedAt);
            
            if (platform.ClosedAt.HasValue)
            {
                _logger.LogError("Попытка создать пикет для закрытой площадки: PlatformId={PlatformId}, ClosedAt={ClosedAt}", platformId, platform.ClosedAt);
                throw new InvalidOperationException("Cannot create picket for closed platform");
            }
            
            // Проверяем уникальность имени пикета в рамках склада с учетом времени
            var recordTime = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
            _logger.LogInformation("Проверяем уникальность имени пикета: Name={Name}, WarehouseId={WarehouseId}, Time={Time}", name, platform.WarehouseId, recordTime);
            var existing = await _picketRepository.GetByNameForUniquenessCheckAsync(platform.WarehouseId, name, recordTime);
            if (existing != null)
            {
                _logger.LogError("Пикет с таким именем уже существует на время {Time}: Name={Name}, WarehouseId={WarehouseId}, ExistingPicketId={ExistingPicketId}, ExistingCreatedAt={ExistingCreatedAt}, ExistingClosedAt={ExistingClosedAt}", 
                    recordTime, name, platform.WarehouseId, existing.Id, existing.CreatedAt, existing.ClosedAt);
                throw new InvalidOperationException($"Picket with name '{name}' already exists in warehouse at time {recordTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                _logger.LogInformation("Пикет с именем {Name} не найден на время {Time}, можно создавать", name, recordTime);
            }
            
            // Создаем пикет
            _logger.LogInformation("Создаем пикет в базе данных: Name={Name}, WarehouseId={WarehouseId}", name, platform.WarehouseId);
            var picket = new Picket
            {
                Name = name,
                WarehouseId = platform.WarehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _picketRepository.AddAsync(picket);
            await _picketRepository.SaveChangesAsync();
            
            _logger.LogInformation("Пикет создан в базе данных: PicketId={PicketId}, Name={Name}, WarehouseId={WarehouseId}", 
                picket.Id, picket.Name, picket.WarehouseId);
            
            // Добавляем пикет к площадке
            _logger.LogInformation("Добавляем пикет к площадке: PlatformId={PlatformId}, PicketId={PicketId}", platformId, picket.Id);
            await _platformPicketRepository.AddPicketsToPlatformAsync(platformId, new[] { picket.Id }, createdAt);
            
            _logger.LogInformation("Пикет успешно добавлен к площадке: PlatformId={PlatformId}, PicketId={PicketId}", platformId, picket.Id);
            
            return picket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пикета: PlatformId={PlatformId}, Name={Name}", platformId, name);
            throw new ApplicationException("Failed to create picket", ex);
        }
    }





    public async Task UpdatePicketAsync(int id, string name)
    {
        var picket = await _picketRepository.GetByIdAsync(id);
        if (picket == null)
            throw new NotFoundException($"Picket with id {id} not found");
        
                 // Проверяем уникальность имени
         if (!string.Equals(picket.Name, name, StringComparison.OrdinalIgnoreCase))
         {
             var existing = await _picketRepository.GetByNameAsync(picket.WarehouseId, name);
             if (existing != null && existing.Id != id)
                 throw new InvalidOperationException($"Picket with name '{name}' already exists in warehouse");
         }
        
        picket.Name = name;
        await _picketRepository.UpdateAsync(picket);
        await _picketRepository.SaveChangesAsync();
    }



    public async Task<IEnumerable<Picket>> GetPicketsByWarehouseAsync(int warehouseId)
    {
        var pickets = await _picketRepository.GetByWarehouseIdAsync(warehouseId);
        return pickets.OrderBy(p => p.Name);
    }

    public async Task<IEnumerable<Picket>> GetPicketsByWarehouseAtTimeAsync(int warehouseId, DateTime time)
    {
        var pickets = await _picketRepository.GetByWarehouseIdAtTimeAsync(warehouseId, time);
        return pickets.OrderBy(p => p.Name);
    }

    public async Task<IEnumerable<Picket>> GetPicketsByPlatformAsync(int platformId)
    {
        var platformPickets = await _platformPicketRepository.GetByPlatformIdAsync(platformId);
        var picketIds = platformPickets.Select(pp => pp.PicketId).ToList();
        
        var pickets = new List<Picket>();
        foreach (var picketId in picketIds)
        {
            var picket = await _picketRepository.GetByIdAsync(picketId);
            if (picket != null)
                pickets.Add(picket);
        }
        
        // Сортируем по имени лексикографически
        return pickets.OrderBy(p => p.Name);
    }

    public async Task<IEnumerable<Picket>> GetPicketsByPlatformAtTimeAsync(int platformId, DateTime time)
    {
        var platformPickets = await _platformPicketRepository.GetByPlatformIdAtTimeAsync(platformId, time);
        var picketIds = platformPickets.Select(pp => pp.PicketId).ToList();
        
        var pickets = new List<Picket>();
        foreach (var picketId in picketIds)
        {
            var picket = await _picketRepository.GetByIdAsync(picketId);
            if (picket != null)
                pickets.Add(picket);
        }
        
        // Сортируем по имени лексикографически
        return pickets.OrderBy(p => p.Name);
    }

    public async Task ClosePicketAsync(int id, DateTime? closedAt = null)
    {
        try
        {
            _logger.LogInformation("Начинаем закрытие пикета {PicketId} на время {CloseTime}", id, closedAt);
            
            var picket = await _picketRepository.GetByIdAsync(id);
            if (picket == null)
                throw new NotFoundException($"Пикет с id {id} не найден");
            
            if (picket.ClosedAt.HasValue)
                throw new InvalidOperationException($"Пикет '{picket.Name}' уже закрыт");
            
            var closeTime = closedAt?.ToUniversalTime() ?? DateTime.UtcNow;
            
            // Получаем все связи пикета с площадками (по PicketId, а не PlatformId!)
            var platformPickets = await _platformPicketRepository.GetByPicketIdAsync(id);
            var affectedPlatforms = new List<int>();
            
            // Закрываем связи пикета с площадками
            foreach (var platformPicket in platformPickets.Where(pp => pp.UnassignedAt == null))
            {
                platformPicket.UnassignedAt = closeTime;
                affectedPlatforms.Add(platformPicket.PlatformId);
            }
            await _platformPicketRepository.SaveChangesAsync();
            
            // Закрываем пикет
            picket.ClosedAt = closeTime;
            await _picketRepository.SaveChangesAsync();
            
            // Проверяем каждую затронутую площадку
            foreach (var platformId in affectedPlatforms)
            {
                var platform = await _platformRepository.GetByIdAsync(platformId);
                if (platform != null && !platform.ClosedAt.HasValue)
                {
                    // Проверяем, остались ли активные пикеты в площадке
                    var activePickets = await _platformPicketRepository.GetByPlatformIdAsync(platformId);
                    var hasActivePickets = activePickets.Any(pp => pp.UnassignedAt == null);
                    
                    if (!hasActivePickets)
                    {
                        _logger.LogInformation("Площадка {PlatformId} не имеет активных пикетов, закрываем её", platformId);
                        platform.ClosedAt = closeTime;
                        await _platformRepository.SaveChangesAsync();
                    }
                }
            }
            
            _logger.LogInformation("Пикет {PicketId} успешно закрыт", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка закрытия пикета {PicketId}", id);
            throw;
        }
    }
} 