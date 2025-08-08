using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Warehouses.backend.Models;

[Index(nameof(Name), IsUnique = true)]
public class CargoType
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")] [Required] [MaxLength(10)]
    public string Name { get; set; } = null!;
}