namespace Warehouses.backend.DTO;

public class CargoDTO
{
    public int Id { get; set; }
    public decimal Remainder { get; set; }
    public decimal Coming { get; set; }
    public decimal Consumption { get; set; }
    public DateTime RecordedAt { get; set; }
    public int PlatformId { get; set; }
    public string GoodType { get; set; } = null!;

}