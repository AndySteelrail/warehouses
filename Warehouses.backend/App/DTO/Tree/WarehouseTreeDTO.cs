namespace Warehouses.backend.DTO.Tree;

public class WarehouseTreeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<PlatformTreeDTO> Platforms { get; set; } = new();
}