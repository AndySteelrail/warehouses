using Warehouses.backend.Models;
using Warehouses.backend.Services;

namespace Warehouses.backend.Repositories.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с типами грузов
/// </summary>
public interface ICargoTypeRepository : IRepository<CargoType>
{
    Task<CargoType?> GetByNameAsync(string name);
}