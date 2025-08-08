using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;
using Warehouses.client.Models.DTO.Tree;

namespace Warehouses.client.Services;

/// <summary>
/// Реализация сервиса для работы со складами
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IApiService _apiService;
    
    public WarehouseService(IApiService apiService)
    {
        _apiService = apiService;
    }
    
    public async Task<List<Warehouse>> GetAllWarehousesAsync(DateTime? time = null)
    {
        string endpoint = "warehouses";
        if (time.HasValue)
        {
            var utcTime = time.Value.ToUniversalTime();
            endpoint += $"?time={utcTime:yyyy-MM-ddTHH:mm:ss}Z";
        }
        
        var warehouses = await _apiService.GetAsync<List<WarehouseDTO>>(endpoint);
        
        if (warehouses == null)
            return new List<Warehouse>();

        return warehouses.Select(MapToModel).ToList();
    }
    
    public async Task<WarehousesTreeDTO> GetWarehousesTreeAsync(DateTime time, int? cargoTypeId = null)
    {
        try
        {
            var utcTime = time.ToUniversalTime();
            string endpoint = $"warehouses/tree?time={utcTime:yyyy-MM-ddTHH:mm:ss}Z";
            
            if (cargoTypeId.HasValue)
            {
                endpoint += $"&cargoTypeId={cargoTypeId.Value}";
            }

            var tree = await _apiService.GetAsync<WarehousesTreeDTO>(endpoint);
            return tree ?? new WarehousesTreeDTO();
        }
        catch (Exception)
        {
            return new WarehousesTreeDTO();
        }
    }
    
    public async Task<Warehouse?> CreateWarehouseAsync(string name, DateTime? createdAt = null)
    {
        var createDto = new CreateWarehouseDTO 
        { 
            Name = name,
            CreatedAt = createdAt?.ToUniversalTime()
        };
        var warehouse = await _apiService.PostAsync<WarehouseDTO>("warehouses", createDto);
        return warehouse != null ? MapToModel(warehouse) : null;
    }
    
    public async Task<bool> UpdateWarehouseAsync(int id, string name)
    {
        var updateDto = new { Name = name };
        return await _apiService.PutAsync($"warehouses/{id}", updateDto);
    }

    
    public async Task<bool> CloseWarehouseAsync(int id, DateTime? closedAt = null)
    {
        try
        {
            var closeDto = new CloseWarehouseDTO 
            { 
                ClosedAt = closedAt?.ToUniversalTime()
            };
            var result = await _apiService.PostAsync<object>($"warehouses/{id}/close", closeDto);
            return result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static Warehouse MapToModel(WarehouseDTO dto)
    {
        return new Warehouse
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }
} 