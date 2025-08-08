namespace Warehouses.backend.DTO;

public class PicketDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int WarehouseId { get; set; }
}