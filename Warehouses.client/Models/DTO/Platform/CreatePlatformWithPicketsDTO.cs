using System;
using System.Collections.Generic;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для создания площадки с пикетами
/// </summary>
public class CreatePlatformWithPicketsDTO
{
    public int WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> PicketIds { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
} 