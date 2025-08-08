using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO;
using Warehouses.backend.DTO.Platform;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для создания площадок с пикетами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlatformCreationController : ControllerBase
{
    private readonly IPlatformCreationService _platformCreationService;
    private readonly ILogger<PlatformCreationController> _logger;

    public PlatformCreationController(
        IPlatformCreationService platformCreationService,
        ILogger<PlatformCreationController> logger)
    {
        _platformCreationService = platformCreationService;
        _logger = logger;
    }

    [HttpPost("with-pickets")]
    public async Task<ActionResult<PlatformDTO>> CreateWithPickets([FromBody] CreatePlatformDTO dto)
    {
        try
        {
            _logger.LogInformation("Получен запрос на создание площадки с пикетами: WarehouseId={WarehouseId}, Name={Name}, PicketIds={PicketIds}", 
                dto.WarehouseId, dto.Name, string.Join(",", dto.PicketIds ?? new List<int>()));
            
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("Некорректные данные в запросе создания площадки: {ModelStateErrors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            if (dto.PicketIds == null || !dto.PicketIds.Any())
            {
                _logger.LogWarning("Не указаны пикеты для создания площадки");
                return BadRequest("Необходимо указать хотя бы один пикет");
            }

            _logger.LogInformation("Создаем площадку через PlatformCreationService: WarehouseId={WarehouseId}, Name={Name}, PicketIds={PicketIds}", 
                dto.WarehouseId, dto.Name, string.Join(",", dto.PicketIds));

            var platform = await _platformCreationService.CreatePlatformWithPicketsAsync(
                dto.WarehouseId, dto.Name, dto.PicketIds, dto.CreatedAt);

            _logger.LogInformation("Площадка успешно создана: PlatformId={PlatformId}, Name={Name}, WarehouseId={WarehouseId}", 
                platform.Id, platform.Name, platform.WarehouseId);

            return Ok(MapToDTO(platform));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Ресурс не найден: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция при создании площадки: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании площадки: WarehouseId={WarehouseId}, Name={Name}", dto.WarehouseId, dto.Name);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }



    private static PlatformDTO MapToDTO(Platform platform)
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
