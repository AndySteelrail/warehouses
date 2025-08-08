using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Warehouses.backend.Models;

[Index(nameof(RecordedAt))]
public class Cargo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("remainder")] [Required] [Precision(18, 3)] [Range(0, double.MaxValue, ErrorMessage = "Остаток по грузу не может быть отрицательным")]
    public decimal Remainder { get; set; }
    
    [Column("coming")] [Required] [Precision(18, 3)] [Range(0, double.MaxValue, ErrorMessage = "Приход груза не может быть отрицательным")]
    public decimal Coming { get; set; }
    
    [Column("consumption")] [Required] [Precision(18, 3)] [Range(0, double.MaxValue, ErrorMessage = "Расход груза не может быть отрицательным")]
    public decimal Consumption { get; set; }
    
    [Column("recorded_at")] [Required]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    [Column("platform_id")] [ForeignKey("Platform")]
    public int PlatformId { get; set; }
    
    [Column("cargo_type_id")] [ForeignKey("CargoType")]
    public int CargoTypeId { get; set; }
    
    public Platform Platform { get; set; } = null!;
    
    public CargoType CargoType { get; set; } = null!;
}