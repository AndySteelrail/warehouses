using Microsoft.EntityFrameworkCore.Storage;

namespace Warehouses.backend.Services;

/// <summary>
/// Базовый интерфейс репозитория
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}