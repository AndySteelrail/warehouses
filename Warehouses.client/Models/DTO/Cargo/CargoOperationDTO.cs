using System;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для операции с грузом
/// </summary>
public class CargoOperationDTO
{
    public int PlatformId { get; set; }
    
    public int GoodTypeId { get; set; }
    
    public decimal? Coming { get; set; }
    
    public decimal? Consumption { get; set; }
    
    public DateTime? RecordedAt { get; set; }
} 