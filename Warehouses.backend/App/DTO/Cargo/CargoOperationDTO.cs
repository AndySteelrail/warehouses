using System.ComponentModel.DataAnnotations;

namespace Warehouses.backend.DTO;

public class CargoOperationDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "ID площадки обязательно")]
    public int PlatformId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Тип груза обязателен")]
    public int GoodTypeId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Значение должно быть положительным")]
    public decimal? Coming { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Значение должно быть положительным")]
    public decimal? Consumption { get; set; }
    
    public DateTime? RecordedAt { get; set; }
}