using System.ComponentModel.DataAnnotations;

namespace Warehouses.backend.DTO;

public class CreateWarehouseDTO
{
    [Required(ErrorMessage = "Название склада обязательно")]
    [StringLength(50, ErrorMessage = "Название склада не должно превышать 50 символов")]
    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}