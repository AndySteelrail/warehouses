using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouses.backend.Models;


public class PlatformPicket
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("platform_id")] [ForeignKey("Platform")]
    public int PlatformId { get; set; }

    [Column("picket_id")] [ForeignKey("Picket")]
    public int PicketId { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; }

    [Column("unassigned_at")]
    public DateTime? UnassignedAt { get; set; }
    
    public Platform Platform { get; set; } = null!;
    public Picket Picket { get; set; } = null!;
}