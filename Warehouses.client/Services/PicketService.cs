using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;
using Microsoft.Extensions.Logging;

namespace Warehouses.client.Services;

/// <summary>
/// Реализация сервиса для работы с пикетами
/// </summary>
public class PicketService : IPicketService
{
    private readonly IApiService _apiService;
    private readonly ILogger<PicketService> _logger;
    
    public PicketService(IApiService apiService, ILogger<PicketService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Picket>> GetPicketsByPlatformAsync(int platformId)
    {
        try
        {
            var picketDtos = await _apiService.GetAsync<List<PicketDTO>>($"pickets/platform/{platformId}");
            return picketDtos?.Select(MapToPicket) ?? Enumerable.Empty<Picket>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Picket>();
        }
    }
    
    public async Task<IEnumerable<Picket>> GetPicketsByPlatformAtTimeAsync(int platformId, DateTime time)
    {
        try
        {
            var utcTime = time.ToUniversalTime();
            var picketDtos = await _apiService.GetAsync<List<PicketDTO>>($"pickets/platform/{platformId}/time?time={utcTime:yyyy-MM-ddTHH:mm:ss}Z");
            return picketDtos?.Select(MapToPicket) ?? Enumerable.Empty<Picket>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Picket>();
        }
    }
    
    public async Task<Picket?> CreatePicketAsync(int? platformId, int? warehouseId, string name, string? newPlatformName = null, DateTime? createdAt = null)
    {
        try
        {
            _logger.LogInformation("Отправляем запрос на создание пикета: PlatformId={PlatformId}, WarehouseId={WarehouseId}, Name={Name}, NewPlatformName={NewPlatformName}", 
                platformId, warehouseId, name, newPlatformName);
            
                            var picketData = new CreatePicketDTO
                {
                    Name = name,
                    PlatformId = platformId,
                    WarehouseId = warehouseId,
                    NewPlatformName = newPlatformName,
                    CreatedAt = createdAt?.ToUniversalTime()
                };

            var result = await _apiService.PostAsync<Picket>("pickets", picketData);
            
            if (result != null)
            {
                _logger.LogInformation("Пикет создан успешно. PicketId={PicketId}", result.Id);
            }
            else
            {
                _logger.LogError("Создание пикета завершилось неудачно. Получен null результат");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пикета: PlatformId={PlatformId}, WarehouseId={WarehouseId}, Name={Name}", platformId, warehouseId, name);
            throw;
        }
    }
    

    public async Task<bool> UpdatePicketNameAsync(int picketId, string name)
    {
        try
        {
            _logger.LogInformation("Отправляем запрос на обновление названия пикета: PicketId={PicketId}, NewName={Name}", picketId, name);
            
            var picketData = new UpdatePicketDTO
            {
                Name = name
            };

            _logger.LogInformation("Данные для обновления пикета: {PicketData}", System.Text.Json.JsonSerializer.Serialize(picketData));

            var success = await _apiService.PutAsync($"pickets/{picketId}", picketData);
            
            if (success)
            {
                _logger.LogInformation("Название пикета обновлено успешно. PicketId={PicketId}, NewName={Name}", picketId, name);
            }
            else
            {
                _logger.LogError("Обновление названия пикета завершилось неудачно");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении названия пикета: PicketId={PicketId}, NewName={Name}", picketId, name);
            throw;
        }
    }


    public async Task<IEnumerable<Picket>> GetPicketsByWarehouseAsync(int warehouseId, DateTime? time = null)
    {
        try
        {
            string endpoint = $"pickets/warehouse/{warehouseId}";
            if (time.HasValue)
            {
                var utcTime = time.Value.ToUniversalTime();
                endpoint += $"?time={utcTime:yyyy-MM-ddTHH:mm:ss}Z";
            }
            
            var picketDtos = await _apiService.GetAsync<List<PicketDTO>>(endpoint);
            return picketDtos?.Select(MapToPicket) ?? Enumerable.Empty<Picket>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Picket>();
        }
    }


    public async Task<bool> ClosePicketAsync(int id, DateTime? closedAt = null)
    {
        try
        {
            var closeDto = new ClosePicketDTO 
            { 
                ClosedAt = closedAt?.ToUniversalTime()
            };
            var result = await _apiService.PostAsync<object>($"pickets/{id}/close", closeDto);
            return result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }


    private static Picket MapToPicket(PicketDTO dto)
    {
        return new Picket
        {
            Id = dto.Id,
            Name = dto.Name,
            WarehouseId = dto.WarehouseId
        };
    }
} 