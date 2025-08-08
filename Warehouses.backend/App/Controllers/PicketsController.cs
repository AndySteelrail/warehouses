using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO;
using Warehouses.backend.DTO.Picket;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories;
using Warehouses.backend.Services;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для управления пикетами
/// </summary>
[ApiController]
[Route("api/pickets")]
public class PicketsController : ControllerBase
{
    private readonly IPicketService _picketService;
    private readonly ILogger<PicketsController> _logger;

    public PicketsController(
        IPicketService picketService,
        ILogger<PicketsController> logger)
    {
        _picketService = picketService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PicketDTO>> Create([FromBody] CreatePicketDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на создание пикета: Name={Name}, PlatformId={PlatformId}, WarehouseId={WarehouseId}, NewPlatformName={NewPlatformName}", 
                dto.Name, dto.PlatformId, dto.WarehouseId, dto.NewPlatformName);
            
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("Некорректные данные в запросе: {ModelStateErrors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Валидация: если PlatformId не указан, то должен быть указан WarehouseId
            if (!dto.PlatformId.HasValue && !dto.WarehouseId.HasValue)
            {
                _logger.LogWarning("Не указан PlatformId, ни WarehouseId");
                return BadRequest(new ErrorResponse("Необходимо указать либо PlatformId, либо WarehouseId", errorCode: "VALIDATION_ERROR"));
            }

            _logger.LogInformation("Создаем пикет через PicketService с транзакцией: Name={Name}, PlatformId={PlatformId}, WarehouseId={WarehouseId}, NewPlatformName={NewPlatformName}", 
                dto.Name, dto.PlatformId, dto.WarehouseId, dto.NewPlatformName);
            
            var picket = await _picketService.CreatePicketWithTransactionAsync(dto.PlatformId, dto.WarehouseId, dto.Name, dto.NewPlatformName, dto.CreatedAt);
            
            _logger.LogInformation("Пикет успешно создан: PicketId={PicketId}, Name={Name}, WarehouseId={WarehouseId}", 
                picket.Id, picket.Name, picket.WarehouseId);
            
            return Ok(new PicketDTO
            {
                Id = picket.Id,
                Name = picket.Name,
                WarehouseId = picket.WarehouseId
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Ресурс не найден: {Message}", ex.Message);
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при создании пикета: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message, errorCode: "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания пикета: Name={Name}, PlatformId={PlatformId}, WarehouseId={WarehouseId}", 
                dto.Name, dto.PlatformId, dto.WarehouseId);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }



    [HttpGet("platform/{platformId}")]
    public async Task<ActionResult<IEnumerable<PicketDTO>>> GetByPlatform(int platformId)
    {
        try
        {
            var pickets = await _picketService.GetPicketsByPlatformAsync(platformId);
            return Ok(pickets.Select(p => new PicketDTO
            {
                Id = p.Id,
                Name = p.Name,
                WarehouseId = p.WarehouseId
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения пикетов площадки {PlatformId}", platformId);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    [HttpGet("platform/{platformId}/time")]
    public async Task<ActionResult<IEnumerable<PicketDTO>>> GetByPlatformAtTime(int platformId, [FromQuery] DateTime time)
    {
        try
        {
            _logger.LogInformation("Получен запрос на получение пикетов площадки на время: PlatformId={PlatformId}, Time={Time}", platformId, time);
            
            var pickets = await _picketService.GetPicketsByPlatformAtTimeAsync(platformId, time);
            
            _logger.LogInformation("Найдено {Count} пикетов для площадки {PlatformId} на время {Time}", pickets.Count(), platformId, time);
            
            return Ok(pickets.Select(p => new PicketDTO
            {
                Id = p.Id,
                Name = p.Name,
                WarehouseId = p.WarehouseId
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения пикетов площадки {PlatformId} на время {Time}", platformId, time);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<PicketDTO>>> GetByWarehouse(int warehouseId, [FromQuery] DateTime? time)
    {
        try
        {
            _logger.LogInformation("Получен запрос на получение пикетов склада: WarehouseId={WarehouseId}, Time={Time}", warehouseId, time);
            
            IEnumerable<Picket> pickets;
            
            if (time.HasValue)
            {
                pickets = await _picketService.GetPicketsByWarehouseAtTimeAsync(warehouseId, time.Value);
            }
            else
            {
                pickets = await _picketService.GetPicketsByWarehouseAsync(warehouseId);
            }
            
            _logger.LogInformation("Найдено {Count} пикетов для склада {WarehouseId}", pickets.Count(), warehouseId);
            
            return Ok(pickets.Select(p => new PicketDTO
            {
                Id = p.Id,
                Name = p.Name,
                WarehouseId = p.WarehouseId
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения пикетов склада {WarehouseId}", warehouseId);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] ClosePicketDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на закрытие пикета: PicketId={PicketId}, ClosedAt={ClosedAt}", id, dto.ClosedAt);
            
            await _picketService.ClosePicketAsync(id, dto.ClosedAt);
            
            _logger.LogInformation("Пикет {PicketId} успешно закрыт на время {ClosedAt}", id, dto.ClosedAt);
            return Ok(new { message = "Пикет успешно закрыт" });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Пикет не найден: {Message}", ex.Message);
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при закрытии пикета: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message, errorCode: "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка закрытия пикета {PicketId}", id);
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePicketDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на обновление пикета: Id={Id}, Name={Name}", id, dto.Name);
            
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("Некорректные данные в запросе обновления: {ModelStateErrors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            await _picketService.UpdatePicketAsync(id, dto.Name);
            
            _logger.LogInformation("Пикет успешно обновлен: Id={Id}, Name={Name}", id, dto.Name);
            
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Пикет не найден при обновлении: {Message}", ex.Message);
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при обновлении пикета: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message, errorCode: "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка обновления пикета по ID {id}");
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }
}