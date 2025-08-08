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
/// Реализация сервиса для работы с площадками
/// </summary>
public class PlatformService : IPlatformService
{
    private readonly IApiService _apiService;
    private readonly ILogger<PlatformService> _logger;
    
    public PlatformService(IApiService apiService, ILogger<PlatformService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Platform>> GetPlatformsByWarehouseAsync(int warehouseId)
    {
        try
        {
            // Получаем все площадки и фильтруем по складу
            var allPlatformDtos = await _apiService.GetAsync<List<PlatformDTO>>("platforms");
            if (allPlatformDtos == null)
                return Enumerable.Empty<Platform>();
            
            var warehousePlatforms = allPlatformDtos.Where(p => p.WarehouseId == warehouseId);
            return warehousePlatforms.Select(MapToPlatform);
        }
        catch (Exception)
        {
            return Enumerable.Empty<Platform>();
        }
    }
    
    public async Task<Platform?> GetPlatformAsync(int platformId)
    {
        try
        {
            var platformDto = await _apiService.GetAsync<PlatformDTO>($"platforms/{platformId}");
            return platformDto != null ? MapToPlatform(platformDto) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
    

    public async Task<bool> DeletePlatformAsync(int platformId)
    {
        try
        {
            await _apiService.DeleteAsync($"platforms/{platformId}");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task<bool> UpdatePlatformNameAsync(int platformId, string name)
    {
        try
        {
            _logger.LogInformation("Отправляем запрос на обновление названия площадки: PlatformId={PlatformId}, NewName={Name}", platformId, name);
            
            var platformData = new UpdatePlatformDTO
            {
                Name = name
            };

            _logger.LogInformation("Данные для обновления площадки: {PlatformData}", System.Text.Json.JsonSerializer.Serialize(platformData));

            var success = await _apiService.PutAsync($"platforms/{platformId}", platformData);
            
            if (success)
            {
                _logger.LogInformation("Название площадки обновлено успешно. PlatformId={PlatformId}, NewName={Name}", platformId, name);
            }
            else
            {
                _logger.LogError("Обновление названия площадки завершилось неудачно");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении названия площадки: PlatformId={PlatformId}, NewName={Name}", platformId, name);
            throw;
        }
    }
    
    public async Task<Platform?> CreatePlatformWithPicketsAsync(int warehouseId, string name, List<int> picketIds, DateTime? createdAt = null)
    {
        try
        {
            var platformData = new CreatePlatformWithPicketsDTO
            {
                WarehouseId = warehouseId,
                Name = name,
                PicketIds = picketIds,
                CreatedAt = createdAt?.ToUniversalTime()
            };
            

            var result = await _apiService.PostAsync<Platform>("platformcreation/with-pickets", platformData);
            
            if (result != null)
            {
                _logger.LogInformation("Площадка с пикетами создана успешно. PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                    result.Id, result.Name, result.WarehouseId);
            }
            else
            {
                _logger.LogError("Создание площадки с пикетами завершилось неудачно.");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки с пикетами: WarehouseId={WarehouseId}, Name={Name}, PicketIds={PicketIds}", 
                warehouseId, name, string.Join(",", picketIds));
            throw;
        }
    }
    
    private static Platform MapToPlatform(PlatformDTO dto)
    {
        return new Platform
        {
            Id = dto.Id,
            Name = dto.Name,
            CreatedAt = dto.CreatedAt,
            ClosedAt = dto.ClosedAt,
            WarehouseId = dto.WarehouseId,
            Pickets = dto.Pickets.Select(p => new Picket
            {
                Id = p.Id,
                Name = p.Name,
                WarehouseId = p.WarehouseId
            }).ToList(),
            CurrentCargo = dto.CurrentCargo != null ? new Cargo
            {
                Id = dto.CurrentCargo.Id,
                Remainder = dto.CurrentCargo.Remainder,
                Coming = dto.CurrentCargo.Coming,
                Consumption = dto.CurrentCargo.Consumption,
                RecordedAt = dto.CurrentCargo.RecordedAt,
                PlatformId = dto.CurrentCargo.PlatformId,
                GoodType = dto.CurrentCargo.GoodType,

            } : null
        };
    }
} 