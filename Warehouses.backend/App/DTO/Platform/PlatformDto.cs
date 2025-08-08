namespace Warehouses.backend.DTO;

public class PlatformDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int WarehouseId { get; set; }
}
