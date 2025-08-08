using Warehouses.backend.DTO.Tree;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;
using Warehouses.backend.Repositories.Interfaces;
using Warehouses.backend.DTO.Warehouse;

namespace Warehouses.backend.Services;

/// <summary>
/// Сервис для управления складами
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IPicketRepository _picketRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly IPlatformPicketRepository _platformPicketRepository;
    private readonly ICargoService _cargoService;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        IPicketRepository picketRepository,
        IPlatformRepository platformRepository,
        IPlatformPicketRepository platformPicketRepository,
        ICargoService cargoService,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _picketRepository = picketRepository;
        _platformRepository = platformRepository;
        _platformPicketRepository = platformPicketRepository;
        _cargoService = cargoService;
        _logger = logger;
    }

    public async Task<Warehouse> CreateWarehouseAsync(string name, DateTime? createdAt = null)
    {
        try
        {
            var existing = await _warehouseRepository.GetByNameAsync(name);
            if (existing != null)
                throw new InvalidOperationException($"Склад с именем '{name}' уже существует");
            
            var warehouse = new Warehouse 
            { 
                Name = name,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };
            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChangesAsync();
            return warehouse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания склада: {Message}", ex.Message);
            throw;
        }
    }



    public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync()
    {
        return await _warehouseRepository.GetWarehousesOnlyAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetWarehousesAtTimeAsync(DateTime time)
    {
        return await _warehouseRepository.GetWarehousesAtTimeAsync(time);
    }



    public async Task<WarehousesTreeDTO> GetWarehousesTreeAsync(DateTime time, int? cargoTypeId = null)
    {
        _logger.LogInformation("Получаем дерево складов: Time={Time}, CargoTypeId={CargoTypeId}", time, cargoTypeId);

        // Получаем все склады на указанное время (без фильтрации по грузу)
        var warehouses = await _warehouseRepository.GetWarehousesAtTimeAsync(time);
        var result = new WarehousesTreeDTO();

        foreach (var warehouse in warehouses.OrderBy(w => w.Name))
        {
            var warehouseDto = new WarehouseTreeDTO
            {
                Id = warehouse.Id,
                Name = warehouse.Name
            };

            // Получаем площадки склада на указанное время
            var platforms = await _platformRepository.GetByWarehouseIdAtTimeAsync(warehouse.Id, time);
            
            foreach (var platform in platforms.OrderBy(p => p.Name))
            {
                try
                {
                    // Получаем информацию о грузе на площадке
                    var cargo = await _cargoService.GetCurrentGoodAsync(platform.Id, time);
                    
                    // Если указан тип груза, проверяем соответствие
                    if (cargoTypeId.HasValue && (cargo.CargoTypeId != cargoTypeId.Value || cargo.Remainder == 0))
                        continue;

                    var platformDto = new PlatformTreeDTO
                    {
                        Id = platform.Id,
                        Name = platform.Name,
                        CargoAmount = cargo.Remainder,
                        CargoType = cargo.CargoType?.Name ?? "Неизвестный тип"
                    };

                    // Получаем пикеты площадки на указанное время
                    var platformPickets = await _platformPicketRepository.GetByPlatformIdAtTimeAsync(platform.Id, time);
                    var picketIds = platformPickets.Select(pp => pp.PicketId).ToList();

                    foreach (var picketId in picketIds)
                    {
                        var picket = await _picketRepository.GetByIdAsync(picketId);
                        if (picket != null)
                        {
                            platformDto.Pickets.Add(new PicketTreeDTO
                            {
                                Id = picket.Id,
                                Name = picket.Name
                            });
                        }
                    }

                    // Сортируем пикеты по имени
                    platformDto.Pickets = platformDto.Pickets.OrderBy(p => p.Name).ToList();
                    warehouseDto.Platforms.Add(platformDto);
                }
                catch (NotFoundException)
                {
                    // Если не указан тип груза, показываем площадку без груза
                    if (!cargoTypeId.HasValue)
                    {
                        var platformDto = new PlatformTreeDTO
                        {
                            Id = platform.Id,
                            Name = platform.Name,
                            CargoAmount = 0,
                            CargoType = "Груза нет"
                        };

                        // Получаем пикеты площадки
                        var platformPickets = await _platformPicketRepository.GetByPlatformIdAtTimeAsync(platform.Id, time);
                        var picketIds = platformPickets.Select(pp => pp.PicketId).ToList();

                        foreach (var picketId in picketIds)
                        {
                            var picket = await _picketRepository.GetByIdAsync(picketId);
                            if (picket != null)
                            {
                                platformDto.Pickets.Add(new PicketTreeDTO
                                {
                                    Id = picket.Id,
                                    Name = picket.Name
                                });
                            }
                        }

                        platformDto.Pickets = platformDto.Pickets.OrderBy(p => p.Name).ToList();
                        warehouseDto.Platforms.Add(platformDto);
                    }
                }
            }

            // Если не указан тип груза, показываем все склады (включая пустые)
            // Если указан тип груза, показываем только склады с соответствующими площадками
            if (!cargoTypeId.HasValue || warehouseDto.Platforms.Any())
            {
                result.Warehouses.Add(warehouseDto);
            }
        }

        _logger.LogInformation("Дерево складов сформировано: {WarehouseCount} складов", result.Warehouses.Count);
        return result;
    }

    public async Task AddPicketToWarehouseAsync(int warehouseId, string picketName, DateTime? createdAt = null)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse == null)
            throw new NotFoundException($"Склад с id {warehouseId} не найден");
        
        var picket = new Picket 
        { 
            Name = picketName, 
            WarehouseId = warehouseId,
            CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
        };
        
        await _picketRepository.AddAsync(picket);
        await _picketRepository.SaveChangesAsync();
    }



    public async Task CloseWarehouseAsync(int id, DateTime? closedAt = null)
    {
        try
        {
            _logger.LogInformation("Начинаем закрытие склада {WarehouseId} на время {CloseTime}", id, closedAt);
            
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new NotFoundException($"Склад с id {id} не найден");
            
            if (warehouse.ClosedAt.HasValue)
                throw new InvalidOperationException($"Склад '{warehouse.Name}' уже закрыт");
            
            var closeTime = closedAt?.ToUniversalTime() ?? DateTime.UtcNow;
            
            // Закрываем все площадки склада
            var platforms = await _platformRepository.GetByWarehouseIdAsync(id);
            foreach (var platform in platforms.Where(p => !p.ClosedAt.HasValue))
            {
                platform.ClosedAt = closeTime;
                await _platformRepository.SaveChangesAsync();
                
                // Закрываем связи пикетов с площадкой
                await _platformRepository.ClosePlatformPicketsAsync(platform.Id, closeTime);
            }
            
            // Закрываем все пикеты склада
            var pickets = await _picketRepository.GetByWarehouseIdAsync(id);
            foreach (var picket in pickets.Where(p => !p.ClosedAt.HasValue))
            {
                picket.ClosedAt = closeTime;
                await _picketRepository.SaveChangesAsync();
            }
            
            // Закрываем сам склад
            warehouse.ClosedAt = closeTime;
            await _warehouseRepository.SaveChangesAsync();
            
            _logger.LogInformation("Склад {WarehouseId} успешно закрыт вместе с {PlatformCount} площадками и {PicketCount} пикетами", 
                id, platforms.Count(p => !p.ClosedAt.HasValue), pickets.Count(p => !p.ClosedAt.HasValue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка закрытия склада {WarehouseId}", id);
            throw;
        }
    }

    public async Task UpdateWarehouseAsync(int id, string name)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            throw new NotFoundException($"Склад с id {id} не найден");
        
        var existing = await _warehouseRepository.GetByNameAsync(name);
        if (existing != null && existing.Id != id)
            throw new InvalidOperationException($"Склад с именем '{name}' уже существует");
        
        warehouse.Name = name;
        await _warehouseRepository.UpdateAsync(warehouse);
        await _warehouseRepository.SaveChangesAsync();
    }
}