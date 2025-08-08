using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы с площадками
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы с площадками
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Получить все площадки склада
    /// </summary>
    /// <param name="warehouseId">Идентификатор склада</param>
    /// <returns>Список площадок</returns>
    Task<IEnumerable<Platform>> GetPlatformsByWarehouseAsync(int warehouseId);

    /// <summary>
    /// Получить площадку по идентификатору
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <returns>Площадка</returns>
    Task<Platform?> GetPlatformAsync(int platformId);


    /// <summary>
    /// Обновить название площадки
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <param name="name">Новое название</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpdatePlatformNameAsync(int platformId, string name);

    /// <summary>
    /// Удалить площадку
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <returns>True если удалено успешно</returns>
    Task<bool> DeletePlatformAsync(int platformId);

    /// <summary>
    /// Создать площадку с пикетами
    /// </summary>
    /// <param name="warehouseId">Идентификатор склада</param>
    /// <param name="name">Название площадки</param>
    /// <param name="picketIds">Список идентификаторов пикетов</param>
    /// <param name="createdAt">Время создания</param>
    /// <returns>Созданная площадка</returns>
    Task<Platform?> CreatePlatformWithPicketsAsync(int warehouseId, string name, List<int> picketIds, DateTime? createdAt = null);
} 