using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO.GoodType;
using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для работы с типами грузов
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CargoTypesController : ControllerBase
{
    private readonly ICargoTypeRepository _cargoTypeRepository;

    public CargoTypesController(ICargoTypeRepository cargoTypeRepository)
    {
        _cargoTypeRepository = cargoTypeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CargoTypeDTO>>> GetAll()
    {
        var types = await _cargoTypeRepository.GetAllAsync();
        return Ok(types.Select(t => new CargoTypeDTO { Id = t.Id, Name = t.Name }));
    }
}
