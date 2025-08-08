using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Warehouses.backend.Models;


[Index(nameof(CreatedAt), nameof(ClosedAt))]
public class Platform
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")] [Required] [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Column("created_at")] [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }
    
    [Column("warehouse_id")] [ForeignKey("Warehouse")]
    public int WarehouseId { get; set; }
    
    public Warehouse Warehouse { get; set; } = null!;
    
    public List<Cargo> Cargoes { get; set; } = new();
    public List<PlatformPicket> PlatformPickets { get; set; } = new();
}