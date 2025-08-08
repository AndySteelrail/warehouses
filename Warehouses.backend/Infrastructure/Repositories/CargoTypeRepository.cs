using Microsoft.EntityFrameworkCore;
using Warehouses.backend.Data;
using Warehouses.backend.Models;
using Warehouses.backend.Repositories.Interfaces;

namespace Warehouses.backend.Repositories;

/// <summary>
/// Репозиторий для работы с типами грузов
/// </summary>
public class CargoTypeRepository : Repository<CargoType>, ICargoTypeRepository
{
    public CargoTypeRepository(AppDbContext context) : base(context) { }

    public async Task<CargoType?> GetByNameAsync(string name) => 
        await _context.CargoTypes.FirstOrDefaultAsync(gt => gt.Name == name);
    
}