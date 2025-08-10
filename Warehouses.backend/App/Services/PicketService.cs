using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;
using Warehouses.backend.Repositories.Interfaces;
using Warehouses.backend.Services;

namespace Warehouses.backend.App.Services;

/// <summary>
/// Сервис для управления пикетами, но создаёт и площадки, если их нет
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

    public async Task<Picket> CreatePicketWithTransactionAsync(
        int? platformId,
        int? warehouseId,
        string picketName,
        string? newPlatformName = null,
        DateTime? createdAt = null)
    {
        // Начинаем транзакцию, т.к. нам нужно создать и пикет, и площадку, либо ничего из этого
        await using var transaction = await _picketRepository.BeginTransactionAsync();
        
        try
        {
            Platform targetPlatform;
            
            if (platformId.HasValue)
            {
                _logger.LogInformation("Создаем пикет на существующей площадке: PlatformId={PlatformId}", platformId.Value);
                targetPlatform = (await _platformRepository.GetByIdAsync(platformId.Value))!;
                if (targetPlatform == null)
                {
                    _logger.LogError("Площадка не найдена: PlatformId={PlatformId}", platformId.Value);
                    throw new NotFoundException($"Платформа с id {platformId.Value} не найдена");
                }
                
                if (targetPlatform.ClosedAt.HasValue)
                {
                    _logger.LogError("Попытка создать пикет для закрытой площадки: PlatformId={PlatformId}", platformId.Value);
                    throw new InvalidOperationException("Невозможно создать пикет на закрытой платформе");
                }
            }
            else
            {
                // Создаем новую площадку
                var logPlatformName = newPlatformName ?? "не указано";
                _logger.LogInformation($"Создаем новую площадку: WarehouseId={warehouseId}, Name={logPlatformName}");
                
                var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId!.Value);
                if (warehouse == null)
                {
                    _logger.LogError($"Склад не найден: WarehouseId={warehouseId}");
                    throw new NotFoundException($"Склад с id {warehouseId} не найден");
                }
                
                // Проверяем уникальность имени площадки
                var platformName = newPlatformName ?? picketName;
                var existingPlatform = await _platformRepository.GetByNameAsync(warehouseId.Value, platformName);
                if (existingPlatform != null)
                {
                    _logger.LogError($"Площадка с таким именем уже существует: Name={platformName}, WarehouseId={warehouseId}");
                    throw new InvalidOperationException($"Платформа с именем '{platformName}' уже есть на складе");
                }
                
                // Создаем новую площадку
                var finalPlatformName = newPlatformName ?? picketName;
                targetPlatform = new Platform
                {
                    Name = finalPlatformName,
                    WarehouseId = warehouseId.Value,
                    CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
                };
                await _platformRepository.AddAsync(targetPlatform);
                await _platformRepository.SaveChangesAsync();
                
                _logger.LogInformation("Новая площадка создана: PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                    targetPlatform.Id, targetPlatform.Name, targetPlatform.WarehouseId);
            }
            
            var recordTime = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
            var finalPicketName = picketName;
            _logger.LogInformation("Проверяем уникальность имени пикета: Name={Name}, WarehouseId={WarehouseId}, Time={Time}", finalPicketName, targetPlatform.WarehouseId, recordTime);
            var existingPicket = await _picketRepository.GetByNameForUniquenessCheckAsync(targetPlatform.WarehouseId, finalPicketName, recordTime);
            if (existingPicket != null)
            {
                _logger.LogError("Пикет с таким именем уже существует на время {Time}: Name={Name}, WarehouseId={WarehouseId}, ExistingPicketId={ExistingPicketId}, ExistingCreatedAt={ExistingCreatedAt}, ExistingClosedAt={ExistingClosedAt}", 
                    recordTime, finalPicketName, targetPlatform.WarehouseId, existingPicket.Id, existingPicket.CreatedAt, existingPicket.ClosedAt);
                var localRecordTime = recordTime.ToLocalTime();
                throw new InvalidOperationException($"Пикет с именем '{finalPicketName}' уже существовал на складе во время {localRecordTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                _logger.LogInformation($"Пикет с именем {finalPicketName} не найден на время {recordTime}, можно создавать");
            }
            
            _logger.LogInformation($"Создаем пикет: Name={finalPicketName}, WarehouseId={targetPlatform.WarehouseId}");
            var picket = new Picket
            {
                Name = finalPicketName,
                WarehouseId = targetPlatform.WarehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _picketRepository.AddAsync(picket);
            await _picketRepository.SaveChangesAsync();
            
            _logger.LogInformation("Пикет создан: PicketId={PicketId}, Name={Name}, WarehouseId={WarehouseId}", 
                picket.Id, picket.Name, picket.WarehouseId);
            
            _logger.LogInformation($"Добавляем пикет к площадке: PlatformId={targetPlatform.Id}, PicketId={picket.Id}");
            await _platformPicketRepository.AddPicketsToPlatformAsync(targetPlatform.Id, new[] { picket.Id }, createdAt);
            
            await transaction.CommitAsync();
            
            _logger.LogInformation($"Транзакция успешно завершена. Пикет создан: PicketId={picket.Id}, PlatformId={targetPlatform.Id}");
            
            return picket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пикета с транзакцией. Откатываем транзакцию.");
            await transaction.RollbackAsync();
            throw;
        }
    }
    

    public async Task UpdatePicketAsync(int id, string name)
    {
        var picket = await _picketRepository.GetByIdAsync(id);
        if (picket == null)
            throw new NotFoundException($"Пикет с id {id} не найден");
        
        if (!string.Equals(picket.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _picketRepository.GetByNameAsync(picket.WarehouseId, name);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException($"Пикет с именем '{name}' уже существует на складе");
            }
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
            
            // Получаем все связи пикета с площадками по PicketId
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