using System.Collections.Generic;

namespace Warehouses.client.Models.DTO.Tree;

/// <summary>
/// DTO для дерева складов
/// </summary>
public class WarehousesTreeDTO
{
    public List<WarehouseTreeDTO> Warehouses { get; set; } = new();
}