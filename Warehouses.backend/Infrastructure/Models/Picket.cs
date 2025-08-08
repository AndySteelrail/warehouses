using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Warehouses.backend.Models;

public class Picket
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")] [Required] [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [Column("warehouse_id")] [ForeignKey("Warehouse")]
    public int WarehouseId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }
    
    public Warehouse Warehouse { get; set; } = null!;
    
    public List<PlatformPicket> PlatformPickets { get; set; } = new();
}