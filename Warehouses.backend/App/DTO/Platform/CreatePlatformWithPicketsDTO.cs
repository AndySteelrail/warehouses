namespace Warehouses.backend.DTO.Platform;

public class CreatePlatformWithPicketsDTO
{
    public int WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> PicketIds { get; set; } = new();
} 