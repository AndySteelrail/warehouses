namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO модель пикета для API
/// </summary>
public class PicketDTO
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int WarehouseId { get; set; }
} 