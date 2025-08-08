using System;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для создания пикета
/// </summary>
public class CreatePicketDTO
{
    public string Name { get; set; } = string.Empty;
    public int? PlatformId { get; set; }
    public int? WarehouseId { get; set; }
    public string? NewPlatformName { get; set; }
    public DateTime? CreatedAt { get; set; }
} 