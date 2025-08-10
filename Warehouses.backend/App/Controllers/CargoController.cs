using Microsoft.AspNetCore.Mvc;
using Warehouses.backend.DTO;
using Warehouses.backend.Exceptions;
using Warehouses.backend.Services;

namespace Warehouses.backend.Controllers;

/// <summary>
/// Контроллер для управления грузами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CargoController : ControllerBase
{
    private readonly ICargoService _cargoService;
    private readonly ILogger<CargoController> _logger;

    public CargoController(ICargoService cargoService, ILogger<CargoController> logger)
    {
        _cargoService = cargoService;
        _logger = logger;
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordOperation([FromBody] CargoOperationDTO dto)
    {
        try
        {
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("Модель невалидна: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            await _cargoService.RecordGoodOperationAsync(
                dto.PlatformId,
                dto.GoodTypeId,
                dto.Coming,
                dto.Consumption,
                dto.RecordedAt);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Не найдено: {Message}", ex.Message);
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Некорректная операция: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(ex.Message, errorCode: "INVALID_OPERATION"));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Ошибка приложения: {Message}", ex.Message);
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new ErrorResponse(innerMessage, errorCode: "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при записи операции с грузом");
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }

    [HttpGet("platform/{platformId}/current")]
    public async Task<ActionResult<CargoDTO>> GetCurrentGood(int platformId, [FromQuery] DateTime? asOfDate)
    {
        try
        {
            var good = await _cargoService.GetCurrentGoodAsync(platformId, asOfDate);
            return Ok(new CargoDTO
            {
                Id = good.Id,
                Remainder = good.Remainder,
                Coming = good.Coming,
                Consumption = good.Consumption,
                RecordedAt = good.RecordedAt,
                PlatformId = good.PlatformId,
                GoodType = good.CargoType.Name
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message, errorCode: "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при получении текущего груза для площадки ID {platformId}");
            return StatusCode(500, new ErrorResponse("Внутренняя ошибка сервера", errorCode: "INTERNAL_ERROR"));
        }
    }
}