using System;
using System.Collections.Generic;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для создания площадки
/// </summary>
public class CreatePlatformDTO
{
    public string Name { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public List<int> PicketIds { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
} 