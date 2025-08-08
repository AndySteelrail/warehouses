using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;

namespace Warehouses.client.Services;

/// <summary>
/// Сервис для работы с грузами
/// </summary>
public class CargoService : ICargoService
{
    private readonly IApiService _apiService;
    private readonly ILogger<CargoService> _logger;

    public CargoService(IApiService apiService, ILogger<CargoService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task AddCargoOperationAsync(int platformId, int cargoTypeId, decimal? coming = null, decimal? consumption = null, DateTime? recordedAt = null)
    {
        try
        {
            var dto = new CargoOperationDTO
            {
                PlatformId = platformId,
                GoodTypeId = cargoTypeId,
                Coming = coming,
                Consumption = consumption,
                RecordedAt = recordedAt?.ToUniversalTime()
            };

            _logger.LogInformation("Отправляем запрос на добавление груза: PlatformId={PlatformId}, GoodTypeId={GoodTypeId}, Coming={Coming}, Consumption={Consumption}", 
                platformId, cargoTypeId, coming, consumption);

            var success = await _apiService.PostAsync<object>("cargo/record", dto);
            
            _logger.LogInformation("Груз успешно добавлен для площадки {PlatformId}", platformId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка API при добавлении груза для площадки {PlatformId}: {Message}", platformId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при добавлении груза для площадки {PlatformId}", platformId);
            throw new Exception($"Неожиданная ошибка: {ex.Message}", ex);
        }
    }


} 