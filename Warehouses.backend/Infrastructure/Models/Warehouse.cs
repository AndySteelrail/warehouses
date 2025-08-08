using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouses.backend.Models;

public class Warehouse
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")] [Required] [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }

    public List<Picket> Pickets { get; set; } = new();
    public List<Platform> Platforms { get; set; } = new();
}