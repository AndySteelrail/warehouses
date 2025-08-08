using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Services;

/// <summary>
/// Сервис для управления грузами
/// </summary>
public class CargoService : ICargoService
{
    private readonly ICargoRepository _cargoRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly ICargoTypeRepository _cargoTypeRepository;
    private readonly ILogger<CargoService> _logger;

    public CargoService(
        ICargoRepository cargoRepository,
        IPlatformRepository platformRepository,
        ICargoTypeRepository cargoTypeRepository,
        ILogger<CargoService> logger)
    {
        _cargoRepository = cargoRepository;
        _platformRepository = platformRepository;
        _cargoTypeRepository = cargoTypeRepository;
        _logger = logger;
    }

    public async Task RecordGoodOperationAsync(
        int platformId, 
        int goodTypeId, 
        decimal? coming = null, 
        decimal? consumption = null,
        DateTime? recordedAt = null)
    {
        try
        {
            // Проверяем существование площадки
            var platform = await _platformRepository.GetByIdAsync(platformId);
            if (platform == null)
                throw new NotFoundException($"Platform with id {platformId} not found");
            
            if (platform.ClosedAt.HasValue)
                throw new InvalidOperationException("Cannot record cargo for closed platform");
            
            // Валидация времени записи относительно времени создания/закрытия площадки
            var recordTime = recordedAt?.ToUniversalTime() ?? DateTime.UtcNow;
            
            if (recordTime < platform.CreatedAt)
            {
                throw new InvalidOperationException($"Нельзя добавить груз на время {recordTime:yyyy-MM-dd HH:mm:ss}, которое раньше создания площадки {platform.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
            
            if (platform.ClosedAt.HasValue && recordTime > platform.ClosedAt.Value)
            {
                throw new InvalidOperationException($"Нельзя добавить груз на время {recordTime:yyyy-MM-dd HH:mm:ss}, которое позже закрытия площадки {platform.ClosedAt.Value:yyyy-MM-dd HH:mm:ss}");
            }
            
            // Проверяем типы грузов и единиц измерения
            var goodType = await _cargoTypeRepository.GetByIdAsync(goodTypeId);
            if (goodType == null)
                throw new NotFoundException($"Good type with id {goodTypeId} not found");
            
            // Проверяем, есть ли уже операция в то же время
            var existingRecordAtTime = await _cargoRepository.GetCargoRecordAtExactTimeAsync(platformId, recordTime);
            if (existingRecordAtTime != null)
            {
                // Объединяем с существующей записью
                await MergeCargoOperationsAsync(existingRecordAtTime, coming, consumption, goodTypeId);
                return;
            }

            // Получаем остаток на время операции
            var previousRecord = await _cargoRepository.GetCargoRecordAtTimeAsync(platformId, recordTime);
            var previousRemainder = previousRecord?.Remainder ?? 0;
            var previousCargoTypeId = previousRecord?.CargoTypeId;
            
            // Валидация типа груза
            if (previousRemainder > 0 && previousCargoTypeId.HasValue && previousCargoTypeId.Value != goodTypeId)
            {
                var previousCargoType = await _cargoTypeRepository.GetByIdAsync(previousCargoTypeId.Value);
                var newCargoType = await _cargoTypeRepository.GetByIdAsync(goodTypeId);
                
                var errorMessage = $"Попытка добавить груз типа '{newCargoType?.Name ?? "Неизвестный"}' на площадку, на которой уже есть груз типа '{previousCargoType?.Name ?? "Неизвестный"}'";
                
                _logger.LogWarning("Валидация не пройдена: {ErrorMessage}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
            
            // Рассчитываем новый остаток
            // Остаток = предыдущий остаток + приход - расход
            var newRemainder = previousRemainder + (coming ?? 0) - (consumption ?? 0);
            
            if (newRemainder < 0)
            {
                var errorMessage = $"Недостаточно груза для расхода. Доступно: {previousRemainder}, запрошено: {Math.Abs(consumption ?? 0)}";
                _logger.LogWarning("Валидация не пройдена: {ErrorMessage}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
            
            // Создаем запись
            var cargoRecord = new Cargo
            {
                Coming = coming ?? 0,
                Consumption = consumption ?? 0,
                Remainder = newRemainder,
                RecordedAt = recordedAt?.ToUniversalTime() ?? DateTime.UtcNow,
                PlatformId = platformId,
                CargoTypeId = goodTypeId
            };
            
            await _cargoRepository.AddAsync(cargoRecord);
            await _cargoRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cargo operation");
            throw; // Просто перебрасываем исходное исключение
        }
    }

    public async Task<Cargo> GetCurrentGoodAsync(int platformId, DateTime? asOfDate = null)
    {
        var platform = await _platformRepository.GetByIdAsync(platformId);
        if (platform == null)
            throw new NotFoundException($"Platform with id {platformId} not found");
        
        var cargoRecord = await _cargoRepository.GetLatestCargoRecordAsync(platformId, asOfDate);
        
        if (cargoRecord == null)
            throw new NotFoundException($"No cargo records found for platform {platformId}");
        
        return cargoRecord;
    }

    private async Task MergeCargoOperationsAsync(Cargo existingRecord, decimal? coming, decimal? consumption, int newCargoTypeId)
    {
        try
        {
            _logger.LogInformation("Объединяем операции груза: ExistingRecordId={RecordId}, Coming={Coming}, Consumption={Consumption}", 
                existingRecord.Id, coming, consumption);

            // Проверяем совместимость типов грузов
            if (existingRecord.CargoTypeId != newCargoTypeId)
            {
                var existingCargoType = await _cargoTypeRepository.GetByIdAsync(existingRecord.CargoTypeId);
                var newCargoType = await _cargoTypeRepository.GetByIdAsync(newCargoTypeId);
                
                throw new InvalidOperationException(
                    $"Нельзя объединить операции с разными типами грузов: '{existingCargoType?.Name ?? "Неизвестный"}' и '{newCargoType?.Name ?? "Неизвестный"}'");
            }

            // Объединяем операции
            var newComing = existingRecord.Coming + (coming ?? 0);
            var newConsumption = existingRecord.Consumption + (consumption ?? 0);
            var newRemainder = existingRecord.Remainder + (coming ?? 0) - (consumption ?? 0);

            if (newRemainder < 0)
            {
                throw new InvalidOperationException(
                    $"Недостаточно груза для расхода. Доступно: {existingRecord.Remainder}, запрошено дополнительно: {Math.Abs(consumption ?? 0)}");
            }

            // Обновляем существующую запись
            existingRecord.Coming = newComing;
            existingRecord.Consumption = newConsumption;
            existingRecord.Remainder = newRemainder;

            await _cargoRepository.SaveChangesAsync();

            _logger.LogInformation("Операции груза объединены: RecordId={RecordId}, NewComing={NewComing}, NewConsumption={NewConsumption}, NewRemainder={NewRemainder}", 
                existingRecord.Id, newComing, newConsumption, newRemainder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при объединении операций груза: RecordId={RecordId}", existingRecord.Id);
            throw;
        }
    }
}