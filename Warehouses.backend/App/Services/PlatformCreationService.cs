using Warehouses.backend.Models;
using Warehouses.backend.Repositories.Interfaces;
using Warehouses.backend.Services;

namespace Warehouses.backend.App.Services;

/// <summary>
/// Интерфейс сервиса для создания площадок с пикетами
/// </summary>
public interface IPlatformCreationService
{
    Task<Platform> CreatePlatformWithPicketsAsync(int warehouseId, string platformName, List<int> picketIds, DateTime? createdAt = null);
}

public class PlatformCreationService : IPlatformCreationService
{
    private readonly IPlatformValidationService _validationService;
    private readonly IPlatformRepository _platformRepository;
    private readonly IPlatformPicketRepository _platformPicketRepository;
    private readonly ICargoRepository _cargoRepository;
    private readonly ICargoTypeRepository _cargoTypeRepository;
    private readonly ILogger<PlatformCreationService> _logger;

    public PlatformCreationService(
        IPlatformValidationService validationService,
        IPlatformRepository platformRepository,
        IPlatformPicketRepository platformPicketRepository,
        ICargoRepository cargoRepository,
        ICargoTypeRepository cargoTypeRepository,
        ILogger<PlatformCreationService> logger)
    {
        _validationService = validationService;
        _platformRepository = platformRepository;
        _platformPicketRepository = platformPicketRepository;
        _cargoRepository = cargoRepository;
        _cargoTypeRepository = cargoTypeRepository;
        _logger = logger;
    }

    public async Task<Platform> CreatePlatformWithPicketsAsync(int warehouseId, string platformName, List<int> picketIds, DateTime? createdAt = null)
    {
        _logger.LogInformation("Начинаем создание площадки: WarehouseId={WarehouseId}, Name={Name}, PicketIds={PicketIds}", 
            warehouseId, platformName, string.Join(",", picketIds));

        // 1. Валидация
        var validationResult = await _validationService.ValidatePlatformCreationAsync(warehouseId, platformName, picketIds, createdAt);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // 2. Транзакция
        await using var transaction = await _platformRepository.BeginTransactionAsync();
        
        try
        {
            // 3. Создаем площадку
            var newPlatform = new Platform
            {
                Name = platformName,
                WarehouseId = warehouseId,
                CreatedAt = createdAt?.ToUniversalTime() ?? DateTime.UtcNow
            };

            await _platformRepository.AddAsync(newPlatform);
            await _platformRepository.SaveChangesAsync();

            _logger.LogInformation("Создана новая площадка с ID: {PlatformId}", newPlatform.Id);

            // 4. Добавляем пикеты к новой площадке
            await _platformPicketRepository.AddPicketsToPlatformAsync(newPlatform.Id, picketIds, createdAt);
            
            _logger.LogInformation("Добавлены пикеты к площадке {PlatformId}: {PicketIds}", 
                newPlatform.Id, string.Join(",", picketIds));

            // 5. Обрабатываем поглощение площадок
            if (validationResult.AbsorptionResult != null)
            {
                await HandlePlatformAbsorptionAsync(newPlatform.Id, validationResult.AbsorptionResult, createdAt);
            }

            // 6. Фиксируем транзакцию
            await transaction.CommitAsync();
            
            _logger.LogInformation("Площадка {PlatformId} успешно создана", newPlatform.Id);
            
            return newPlatform;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки, откатываем транзакцию");
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task HandlePlatformAbsorptionAsync(int newPlatformId, PlatformAbsorptionResult absorptionResult, DateTime? createdAt = null)
    {
        _logger.LogInformation("Обрабатываем поглощение площадок: FullyAbsorbed={FullyAbsorbed}, PartiallyAbsorbed={PartiallyAbsorbed}", 
            string.Join(",", absorptionResult.FullyAbsorbedPlatforms), 
            string.Join(",", absorptionResult.PartiallyAbsorbedPlatforms));

        // Обрабатываем полностью поглощенные площадки
        if (absorptionResult.FullyAbsorbedPlatforms.Any())
        {
            await HandleFullyAbsorbedPlatformsAsync(newPlatformId, absorptionResult.FullyAbsorbedPlatforms, createdAt);
        }

        // Обрабатываем частично поглощенные площадки
        if (absorptionResult.PartiallyAbsorbedPlatforms.Any())
        {
            await HandlePartiallyAbsorbedPlatformsAsync(newPlatformId, absorptionResult.PartiallyAbsorbedPlatforms);
        }
    }

    private async Task HandleFullyAbsorbedPlatformsAsync(int newPlatformId, List<int> absorbedPlatformIds, DateTime? createdAt = null)
    {
        _logger.LogInformation("Обрабатываем полностью поглощенные площадки: {PlatformIds}", string.Join(",", absorbedPlatformIds));

        // Проверяем типы грузов на поглощенных площадках
        var cargoTypes = new HashSet<int>();
        var platformCargos = new Dictionary<int, Cargo>();

        foreach (var platformId in absorbedPlatformIds)
        {
            var latestCargo = await _cargoRepository.GetLatestCargoRecordAsync(platformId);
            if (latestCargo != null && latestCargo.Remainder > 0)
            {
                cargoTypes.Add(latestCargo.CargoTypeId);
                platformCargos[platformId] = latestCargo;
            }
        }

        // Если есть грузы разных типов, выбрасываем исключение
        if (cargoTypes.Count > 1)
        {
            var cargoTypeNames = new List<string>();
            foreach (var cargoTypeId in cargoTypes)
            {
                var cargoType = await _cargoTypeRepository.GetByIdAsync(cargoTypeId);
                cargoTypeNames.Add(cargoType!.Name);
            }
            
            throw new InvalidOperationException(
                $"Невозможно объединить площадки с разными типами грузов: {string.Join(", ", cargoTypeNames)}");
        }

        // Закрываем старые площадки
        var closeTime = createdAt?.ToUniversalTime() ?? DateTime.UtcNow;
        foreach (var platformId in absorbedPlatformIds)
        {
            var platform = await _platformRepository.GetByIdAsync(platformId);
            if (platform != null)
            {
                platform.ClosedAt = closeTime;
                await _platformRepository.SaveChangesAsync();
                
                _logger.LogInformation("Закрыта площадка {PlatformId}", platformId);
            }
        }

        // Переносим грузы на новую площадку
        if (cargoTypes.Count == 1)
        {
            var cargoTypeId = cargoTypes.First();
            var totalRemainder = platformCargos.Values.Sum(c => c.Remainder);
            
            if (totalRemainder > 0)
            {
                var cargoRecord = new Cargo
                {
                    Coming = totalRemainder,
                    Consumption = 0,
                    Remainder = totalRemainder,
                    RecordedAt = closeTime,
                    PlatformId = newPlatformId,
                    CargoTypeId = cargoTypeId
                };

                await _cargoRepository.AddAsync(cargoRecord);
                await _cargoRepository.SaveChangesAsync();
                
                _logger.LogInformation("Перенесен груз на новую площадку {PlatformId}: {Remainder} единиц типа {CargoTypeId}", 
                    newPlatformId, totalRemainder, cargoTypeId);
            }
        }
    }

    private async Task HandlePartiallyAbsorbedPlatformsAsync(int newPlatformId, List<int> partiallyAbsorbedPlatformIds)
    {
        _logger.LogInformation("Обрабатываем частично поглощенные площадки: {PlatformIds}", string.Join(",", partiallyAbsorbedPlatformIds));

        // Для частично поглощенных площадок просто удаляем связи с пикетами
        // Грузы остаются на оставшихся пикетах
        foreach (var platformId in partiallyAbsorbedPlatformIds)
        {
            var platformPicketIds = await _platformPicketRepository.GetPicketIdsByPlatformIdAsync(platformId);
            var newPlatformPicketIds = await _platformPicketRepository.GetPicketIdsByPlatformIdAsync(newPlatformId);
            
            var intersection = platformPicketIds.Intersect(newPlatformPicketIds).ToList();
            
            if (intersection.Any())
            {
                await _platformPicketRepository.RemovePicketsFromPlatformAsync(platformId, intersection);
                
                _logger.LogInformation("Удалены пикеты {PicketIds} из площадки {PlatformId}", 
                    string.Join(",", intersection), platformId);
            }
        }
    }
} 