using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;

namespace Warehouses.client.Services;

/// <summary>
/// Реализация сервиса для работы со справочниками
/// </summary>
public class CargoTypesService : IReferenceService
{
    private readonly IApiService _apiService;
    
    public CargoTypesService(IApiService apiService)
    {
        _apiService = apiService;
    }
    
    public async Task<List<CargoType>> GetCargoTypesAsync()
    {
        var cargoTypes = await _apiService.GetAsync<List<CargoTypeDTO>>("cargotypes");
        
        if (cargoTypes == null)
            return new List<CargoType>();

        return cargoTypes.Select(MapToModel).ToList();
    }
    
    private static CargoType MapToModel(CargoTypeDTO dto)
    {
        return new CargoType
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }


} 