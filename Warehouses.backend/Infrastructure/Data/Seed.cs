using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouses.backend.Models;

namespace Warehouses.backend.Data;

/// <summary>
/// Класс наполнения БД первоначальными данными
/// </summary>
public static class Seed
{
    private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Seed");
    
    // Метод инициализации вызывает цепочку методов наполнения таблиц
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.Warehouses.AnyAsync())
        {
            Logger.LogInformation("Все таблицы инициализированы");
            return;
        }
        
        var warehouses = InitializeWarehouses();
        await db.Warehouses.AddRangeAsync(warehouses);
        await db.SaveChangesAsync();
        Logger.LogInformation("Таблица складов инициализирована");
        
        var pickets = InitializePickets();
        await db.Pickets.AddRangeAsync(pickets);
        await db.SaveChangesAsync();
        Logger.LogInformation("Таблица пикетов инициализирована");
        
        var platforms = InitializePlatforms();
        await db.Platforms.AddRangeAsync(platforms);
        await db.SaveChangesAsync();
        Logger.LogInformation("Таблица площадок инициализирована");
        
        var platformsPickets = InitializePlatformPickets();
        await db.PlatformPickets.AddRangeAsync(platformsPickets);
        await db.SaveChangesAsync();
        Logger.LogInformation("Промежуточная таблица \"многие ко многим\" для площадок и пикетов инициализирована");
        
        var goodTypes = InitializeCargoTypes();
        await db.CargoTypes.AddRangeAsync(goodTypes);
        await db.SaveChangesAsync();
        Logger.LogInformation("Таблица типов грузов инициализирована");
        
        var goods = InitializeCargoes();
        await db.Cargoes.AddRangeAsync(goods);
        await db.SaveChangesAsync();
        Logger.LogInformation("Таблица грузов инициализирована");
    }
    
    private static List<Warehouse> InitializeWarehouses()
    {
        return new List<Warehouse>
        {
            new Warehouse { Name = "Склад 1", CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Warehouse { Name = "Склад 2", CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) }
        };
    }
    
    private static List<Picket> InitializePickets()
    {
        return new List<Picket>
        {
            new Picket { Name = "101", WarehouseId = 1, CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Picket { Name = "102", WarehouseId = 1, CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Picket { Name = "103", WarehouseId = 1, CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Picket { Name = "104", WarehouseId = 1, CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Picket { Name = "105", WarehouseId = 1, CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc) },
            new Picket { Name = "201", WarehouseId = 2, CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) },
            new Picket { Name = "202", WarehouseId = 2, CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) },
            new Picket { Name = "203", WarehouseId = 2, CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) },
            new Picket { Name = "204", WarehouseId = 2, CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) },
            new Picket { Name = "205", WarehouseId = 2, CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc) }
        };
    }
    
    private static List<Platform> InitializePlatforms()
    {
        return new List<Platform>
        {
            new Platform { Name = "101 - 104", CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), ClosedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), WarehouseId = 1 },
            new Platform { Name = "105", CreatedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), ClosedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), WarehouseId = 1 },
            new Platform { Name = "201 - 202", CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), WarehouseId = 2 },
            new Platform { Name = "203 - 205", CreatedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), WarehouseId = 2 },
            new Platform { Name = "101 - 103", CreatedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), WarehouseId = 1 },
            new Platform { Name = "104 - 105", CreatedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), WarehouseId = 1 },
        };
    }
    
    private static List<PlatformPicket> InitializePlatformPickets()
    {
        return new List<PlatformPicket>
        {
            new PlatformPicket { PlatformId = 1, PicketId = 1, AssignedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), UnassignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc) },
            new PlatformPicket { PlatformId = 1, PicketId = 2, AssignedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), UnassignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc) },
            new PlatformPicket { PlatformId = 1, PicketId = 3, AssignedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), UnassignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc) },
            new PlatformPicket { PlatformId = 1, PicketId = 4, AssignedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), UnassignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc) },
            
            new PlatformPicket { PlatformId = 2, PicketId = 5, AssignedAt = new DateTime(2025, 5, 30, 22, 4, 0, DateTimeKind.Utc), UnassignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc) },
            
            new PlatformPicket { PlatformId = 3, PicketId = 6, AssignedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 3, PicketId = 7, AssignedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), UnassignedAt = null },
            
            new PlatformPicket { PlatformId = 4, PicketId = 8, AssignedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 4, PicketId = 9, AssignedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 4, PicketId = 10, AssignedAt = new DateTime(2025, 6, 2, 13, 4, 25, DateTimeKind.Utc), UnassignedAt = null },
            
            new PlatformPicket { PlatformId = 5, PicketId = 1, AssignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 5, PicketId = 2, AssignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 5, PicketId = 3, AssignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), UnassignedAt = null },
            
            new PlatformPicket { PlatformId = 6, PicketId = 4, AssignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), UnassignedAt = null },
            new PlatformPicket { PlatformId = 6, PicketId = 5, AssignedAt = new DateTime(2025, 7, 2, 11, 24, 0, DateTimeKind.Utc), UnassignedAt = null }
        };
    }
    
    private static List<CargoType> InitializeCargoTypes()
    {
        return new List<CargoType>
        {
            new CargoType { Name = "ДОМСШ 0-50" },
            new CargoType { Name = "БР 0-200" }
        };
    }
    
    private static List<Cargo> InitializeCargoes()
    {
        return new List<Cargo>
        {
            new Cargo { Remainder = 53_000, Coming = 53_000, Consumption = 0, RecordedAt = new DateTime(2025, 5, 31, 08, 12, 30, DateTimeKind.Utc), PlatformId = 1, CargoTypeId = 1 },
            new Cargo { Remainder = 5_000, Coming = 5_000, Consumption = 0, RecordedAt = new DateTime(2025, 6, 01, 07, 34, 45, DateTimeKind.Utc), PlatformId = 2, CargoTypeId = 2 },
            
            new Cargo { Remainder = 8_000, Coming = 8_000, Consumption = 0, RecordedAt = new DateTime(2025, 6, 03, 08, 12, 30, DateTimeKind.Utc), PlatformId = 3, CargoTypeId = 2 },
            new Cargo { Remainder = 6_000, Coming = 6_000, Consumption = 0, RecordedAt = new DateTime(2025, 6, 04, 09, 33, 00, DateTimeKind.Utc), PlatformId = 4, CargoTypeId = 1 },
            new Cargo { Remainder = 9_000, Coming = 9_000, Consumption = 0, RecordedAt = new DateTime(2025, 6, 04, 15, 12, 45, DateTimeKind.Utc), PlatformId = 4, CargoTypeId = 1 },
            
            new Cargo { Remainder = 0, Coming = 0, Consumption = 53_000, RecordedAt = new DateTime(2025, 6, 30, 18, 02, 30, DateTimeKind.Utc), PlatformId = 1, CargoTypeId = 1 },
            new Cargo { Remainder = 0, Coming = 0, Consumption = 5_000, RecordedAt = new DateTime(2025, 6, 30, 21, 14, 45, DateTimeKind.Utc), PlatformId = 2, CargoTypeId = 2 },
            
            new Cargo { Remainder = 24_000, Coming = 24_000, Consumption = 0, RecordedAt = new DateTime(2025, 7, 03, 12, 42, 30, DateTimeKind.Utc), PlatformId = 5, CargoTypeId = 1 },
            new Cargo { Remainder = 17_500, Coming = 17_500, Consumption = 0, RecordedAt = new DateTime(2025, 7, 03, 14, 54, 45, DateTimeKind.Utc), PlatformId = 6, CargoTypeId = 1 }
        };
    }
}