using System;
using System.Collections.Generic;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO модель площадки для API
/// </summary>
public class PlatformDTO
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public int WarehouseId { get; set; }
    
    public List<PicketDTO> Pickets { get; set; } = new();
    
    public CargoDTO? CurrentCargo { get; set; }
} 