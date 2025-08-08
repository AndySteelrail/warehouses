using System.ComponentModel.DataAnnotations;

namespace Warehouses.backend.DTO;

public class CreatePicketDTO
{
    [Required(ErrorMessage = "Название пикета обязательно")]
    [StringLength(50, ErrorMessage = "Название пикета не должно превышать 50 символов")]
    public string Name { get; set; } = null!;
    
    public int? PlatformId { get; set; }
    
    public int? WarehouseId { get; set; }
    
    [StringLength(50, ErrorMessage = "Название площадки не должно превышать 50 символов")]
    public string? NewPlatformName { get; set; }

    public DateTime? CreatedAt { get; set; }
}