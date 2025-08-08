using System.Collections.Generic;

namespace Warehouses.client.Models.DTO.Tree;

/// <summary>
/// DTO для склада, как части дерева
/// </summary>
public class WarehouseTreeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<PlatformTreeDTO> Platforms { get; set; } = new();
}