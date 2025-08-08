using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO;
using Warehouses.backend.DTO.Tree;
using Warehouses.backend.DTO.Warehouse;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Services;
using Warehouses.backend.Models;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для управления складами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehousesController> _logger;

    public WarehousesController(
        IWarehouseService warehouseService,
        ILogger<WarehousesController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDTO>>> GetAll([FromQuery] DateTime? time)
    {
        try
        {
            IEnumerable<Warehouse> warehouses;
            
            if (time.HasValue)
            {
                warehouses = await _warehouseService.GetWarehousesAtTimeAsync(time.Value);
            }
            else
            {
                warehouses = await _warehouseService.GetAllWarehousesAsync();
            }
            
            return Ok(warehouses
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseDTO
                {
                    Id = w.Id,
                    Name = w.Name
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка складов");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }



    [HttpGet("tree")]
    public async Task<ActionResult<WarehousesTreeDTO>> GetTree(
        [FromQuery] DateTime time,
        [FromQuery] int? cargoTypeId = null)
    {
        try
        {
            var tree = await _warehouseService.GetWarehousesTreeAsync(time, cargoTypeId);
            return Ok(tree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении дерева складов");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDTO>> Create([FromBody] CreateWarehouseDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на создание склада: Name={Name}, CreatedAt={CreatedAt}", 
                dto.Name, dto.CreatedAt);
            
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var warehouse = await _warehouseService.CreateWarehouseAsync(dto.Name, dto.CreatedAt);
            return Ok(new WarehouseDTO
            {
                Id = warehouse.Id,
                Name = warehouse.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании склада: {Message}", ex.Message);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }



    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseWarehouseDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на закрытие склада: WarehouseId={WarehouseId}, ClosedAt={ClosedAt}", id, dto.ClosedAt);
            
            await _warehouseService.CloseWarehouseAsync(id, dto.ClosedAt);
            
            _logger.LogInformation("Склад {WarehouseId} успешно закрыт на время {ClosedAt}", id, dto.ClosedAt);
            return Ok(new { message = "Склад успешно закрыт" });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Склад не найден: {Message}", ex.Message);
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при закрытии склада: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message, errorCode: "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка закрытия склада {WarehouseId}", id);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            await _warehouseService.UpdateWarehouseAsync(id, dto.Name);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обновлении склада ID {id}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}