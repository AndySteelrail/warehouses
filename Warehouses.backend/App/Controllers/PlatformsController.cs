using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO;
using Warehouses.backend.DTO.Platform;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для управления площадками
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformService _platformService;
    private readonly ILogger<PlatformsController> _logger;

    public PlatformsController(
        IPlatformService platformService,
        ILogger<PlatformsController> logger)
    {
        _platformService = platformService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PlatformDTO>> Create([FromBody] CreatePlatformDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на создание площадки: WarehouseId={WarehouseId}, Name={Name}, PicketIds={PicketIds}", 
                dto.WarehouseId, dto.Name, string.Join(",", dto.PicketIds));
            
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("Некорректные данные в запросе создания площадки: {ModelStateErrors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Создаем площадку через PlatformService: WarehouseId={WarehouseId}, Name={Name}", dto.WarehouseId, dto.Name);
            
            var platform = await _platformService.CreatePlatformAsync(
                dto.WarehouseId, dto.Name, dto.PicketIds, dto.CreatedAt);

            _logger.LogInformation("Площадка успешно создана: PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                platform.Id, platform.Name, platform.WarehouseId);

            return CreatedAtAction(nameof(GetById), new { id = platform.Id }, MapToDTO(platform));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Площадка не найдена: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при создании площадки: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки: WarehouseId={WarehouseId}, Name={Name}", dto.WarehouseId, dto.Name);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlatformDTO>>> GetAll()
    {
        try
        {
            var platforms = await _platformService.GetAllPlatformsAsync();
            return Ok(platforms.Select(MapToDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка площадок");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlatformDTO>> GetById(int id)
    {
        try
        {
            var platform = await _platformService.GetPlatformAsync(id);
            return Ok(MapToDTO(platform));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при получении площадки ID {id}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlatformDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            await _platformService.UpdatePlatformAsync(id, dto.Name);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обновлении площадки ID {id}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    

    private PlatformDTO MapToDTO(Platform platform)
    {
        return new PlatformDTO
        {
            Id = platform.Id,
            Name = platform.Name,
            CreatedAt = platform.CreatedAt,
            ClosedAt = platform.ClosedAt,
            WarehouseId = platform.WarehouseId
        };
    }
}