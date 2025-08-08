using System;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для создания склада
/// </summary>
public class CreateWarehouseDTO
{
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
} 