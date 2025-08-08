using System.ComponentModel.DataAnnotations;

namespace Warehouses.backend.DTO;

public class CreatePlatformDTO
{
    [Required(ErrorMessage = "Название площадки обязательно")]
    [StringLength(50, ErrorMessage = "Название площадки не должно превышать 50 символов")]
    public string Name { get; set; } = null!;
    
    [Range(1, int.MaxValue, ErrorMessage = "ID склада обязательно")]
    public int WarehouseId { get; set; }
    
    [MinLength(1, ErrorMessage = "Должен быть хотя бы один пикет")]
    public List<int> PicketIds { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
}