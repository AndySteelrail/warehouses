using System.Collections.Generic;

namespace Warehouses.client.Models.DTO.Tree;

/// <summary>
/// DTO для площадки, как части дерева
/// </summary>
public class PlatformTreeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal CargoAmount { get; set; }
    public string CargoType { get; set; } = null!;
    public List<PicketTreeDTO> Pickets { get; set; } = new();
}