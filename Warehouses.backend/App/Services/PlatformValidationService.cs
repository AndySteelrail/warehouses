using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Services;

/// <summary>
/// Интерфейс сервиса валидации площадок
/// </summary>
public interface IPlatformValidationService
{
    Task<ValidationResult> ValidatePlatformCreationAsync(int warehouseId, string platformName, List<int> picketIds);
}

public class PlatformValidationService : IPlatformValidationService
{
    private readonly IPlatformPicketRepository _platformPicketRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly ILogger<PlatformValidationService> _logger;

    public PlatformValidationService(
        IPlatformPicketRepository platformPicketRepository,
        IPlatformRepository platformRepository,
        ILogger<PlatformValidationService> logger)
    {
        _platformPicketRepository = platformPicketRepository;
        _platformRepository = platformRepository;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidatePlatformCreationAsync(int warehouseId, string platformName, List<int> picketIds)
    {
        // 1. Проверка на пустые пикеты
        if (!picketIds.Any())
        {
            return ValidationResult.Error("Необходимо выбрать хотя бы один пикет для площадки");
        }

        // 2. Проверка на уникальность имени площадки
        var existingPlatformByName = await _platformRepository.GetByNameAsync(warehouseId, platformName);
        if (existingPlatformByName != null)
        {
            return ValidationResult.Error($"Площадка с именем '{platformName}' уже существует");
        }

        // 3. Проверка непрерывности пикетов
        var areSequential = await _platformPicketRepository.ArePicketsSequentialAsync(picketIds);
        if (!areSequential)
        {
            return ValidationResult.Error("Пикеты в новой площадке должны идти последовательно");
        }

        // 4. Получаем карту существующих площадок
        var platformMapping = await _platformPicketRepository.GetPlatformPicketsMappingAsync(warehouseId);
        
        // 5. Проверка на полное совпадение с существующей площадкой
        foreach (var kvp in platformMapping)
        {
            var existingPicketIds = kvp.Value;
            if (picketIds.Count == existingPicketIds.Count && 
                picketIds.All(id => existingPicketIds.Contains(id)))
            {
                var existingPlatform = await _platformRepository.GetByIdAsync(kvp.Key);
                return ValidationResult.Error($"Выбранные пикеты уже содержит площадка '{existingPlatform?.Name}', используйте её");
            }
        }

        // 6. Проверка на разрыв существующих площадок
        var breakValidation = await ValidateNoPlatformBreakAsync(platformMapping, picketIds);
        if (!breakValidation.IsValid)
        {
            return breakValidation;
        }

        // 7. Анализ поглощения площадок
        var absorptionResult = await AnalyzePlatformAbsorptionAsync(platformMapping, picketIds);
        
        return ValidationResult.Success(absorptionResult);
    }

    private async Task<ValidationResult> ValidateNoPlatformBreakAsync(Dictionary<int, List<int>> platformMapping, List<int> newPicketIds)
    {
        _logger.LogInformation("Проверяем разрыв площадок. Новые пикеты: {NewPicketIds}", string.Join(",", newPicketIds));
        
        foreach (var kvp in platformMapping)
        {
            var platformId = kvp.Key;
            var platformPicketIds = kvp.Value.OrderBy(id => id).ToList();
            
            _logger.LogInformation("Проверяем площадку {PlatformId} с пикетами: {PlatformPicketIds}", platformId, string.Join(",", platformPicketIds));
            
            // Проверяем, не разрываем ли мы эту площадку
            var intersection = platformPicketIds.Intersect(newPicketIds).ToList();
            var remaining = platformPicketIds.Except(newPicketIds).ToList();
            
            _logger.LogInformation("Пересечение: {Intersection}, Оставшиеся: {Remaining}", 
                string.Join(",", intersection), string.Join(",", remaining));
            
            if (intersection.Any() && remaining.Any())
            {
                // Есть пересечение, но не все пикеты площадки включены
                // Проверяем, не разрываем ли мы последовательность
                
                // Сортируем пересечение и оставшиеся пикеты
                var sortedIntersection = intersection.OrderBy(id => id).ToList();
                var sortedRemaining = remaining.OrderBy(id => id).ToList();
                
                // Проверяем, что оставшиеся пикеты образуют непрерывную последовательность
                var isRemainingSequential = true;
                for (int i = 1; i < sortedRemaining.Count; i++)
                {
                    if (sortedRemaining[i] != sortedRemaining[i - 1] + 1)
                    {
                        isRemainingSequential = false;
                        break;
                    }
                }
                
                _logger.LogInformation("Оставшиеся пикеты {Remaining} образуют непрерывную последовательность: {IsSequential}", 
                    string.Join(",", sortedRemaining), isRemainingSequential);
                
                // Если оставшиеся пикеты НЕ образуют непрерывную последовательность - это разрыв
                if (!isRemainingSequential)
                {
                    var existingPlatform = await _platformRepository.GetByIdAsync(platformId);
                    _logger.LogWarning("Обнаружен разрыв площадки {PlatformName} (ID: {PlatformId})", existingPlatform?.Name, platformId);
                    return ValidationResult.Error(
                        $"Недопустимо разрывать последовательные пикеты существующей площадки '{existingPlatform?.Name}' на несколько частей. " +
                        "Создайте новую площадку, включив в неё не только пикеты в середине существующей площадки, но и пикеты с одной из оставшихся сторон.");
                }
            }
        }
        
        _logger.LogInformation("Проверка разрыва площадок завершена успешно");
        return ValidationResult.Success();
    }

    private Task<PlatformAbsorptionResult> AnalyzePlatformAbsorptionAsync(Dictionary<int, List<int>> platformMapping, List<int> newPicketIds)
    {
        var result = new PlatformAbsorptionResult();
        
        foreach (var kvp in platformMapping)
        {
            var platformId = kvp.Key;
            var platformPicketIds = kvp.Value;
            
            var intersection = platformPicketIds.Intersect(newPicketIds).ToList();
            
            if (intersection.Any())
            {
                // Есть пересечение с этой площадкой
                if (intersection.Count == platformPicketIds.Count)
                {
                    // Полное поглощение площадки
                    result.FullyAbsorbedPlatforms.Add(platformId);
                }
                else
                {
                    // Частичное поглощение
                    result.PartiallyAbsorbedPlatforms.Add(platformId);
                }
            }
        }
        
        return Task.FromResult(result);
    }
}

/// <summary>
/// Результат валидации площадки
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public PlatformAbsorptionResult? AbsorptionResult { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null, PlatformAbsorptionResult? absorptionResult = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        AbsorptionResult = absorptionResult;
    }

    public static ValidationResult Success(PlatformAbsorptionResult? absorptionResult = null)
        => new ValidationResult(true, absorptionResult: absorptionResult);

    public static ValidationResult Error(string message)
        => new ValidationResult(false, message);
}