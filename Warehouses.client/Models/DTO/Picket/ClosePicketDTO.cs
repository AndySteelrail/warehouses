using System;

namespace Warehouses.client.Models.DTO;

/// <summary>
/// DTO для закрытия пикета
/// </summary>
public class ClosePicketDTO
{
    public DateTime? ClosedAt { get; set; }
}
