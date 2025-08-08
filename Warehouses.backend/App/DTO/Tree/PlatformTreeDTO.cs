namespace Warehouses.backend.DTO.Tree;

public class PlatformTreeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal CargoAmount { get; set; }
    public string CargoType { get; set; } = null!;
    public List<PicketTreeDTO> Pickets { get; set; } = new();
}