using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Models;

namespace Warehouses.backend.Data;

/// <summary>
/// Контекст БД приложения
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Picket> Pickets => Set<Picket>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<PlatformPicket> PlatformPickets => Set<PlatformPicket>();
    public DbSet<Cargo> Cargoes => Set<Cargo>();
    public DbSet<CargoType> CargoTypes => Set<CargoType>();
    
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}