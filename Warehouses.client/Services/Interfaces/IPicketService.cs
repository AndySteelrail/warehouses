using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы с пикетами
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы с пикетами
/// </summary>
public interface IPicketService
{
    /// <summary>
    /// Получить все пикеты площадки
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <returns>Список пикетов</returns>
    Task<IEnumerable<Picket>> GetPicketsByPlatformAsync(int platformId);

    /// <summary>
    /// Получить все пикеты площадки на определенное время
    /// </summary>
    /// <param name="platformId">Идентификатор площадки</param>
    /// <param name="time">Время для фильтрации</param>
    /// <returns>Список пикетов</returns>
    Task<IEnumerable<Picket>> GetPicketsByPlatformAtTimeAsync(int platformId, DateTime time);



    /// <summary>
    /// Создать новый пикет
    /// </summary>
    /// <param name="platformId">Идентификатор площадки (может быть null для создания новой площадки)</param>
    /// <param name="warehouseId">Идентификатор склада (нужен если platformId не указан)</param>
    /// <param name="name">Название пикета</param>
    /// <param name="newPlatformName">Название новой площадки (если platformId не указан)</param>
    /// <param name="createdAt">Время создания</param>
    /// <returns>Созданный пикет</returns>
    Task<Picket?> CreatePicketAsync(int? platformId, int? warehouseId, string name, string? newPlatformName = null, DateTime? createdAt = null);

    /// <summary>
    /// Обновить название пикета
    /// </summary>
    /// <param name="picketId">Идентификатор пикета</param>
    /// <param name="name">Новое название</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpdatePicketNameAsync(int picketId, string name);

    /// <summary>
    /// Удалить пикет
    /// </summary>
    /// <param name="picketId">Идентификатор пикета</param>
    /// <returns>True если удалено успешно</returns>


    /// <summary>
    /// Получить все пикеты склада
    /// </summary>
    /// <param name="warehouseId">Идентификатор склада</param>
    /// <param name="time">Время для фильтрации (опционально)</param>
    /// <returns>Список пикетов</returns>
    Task<IEnumerable<Picket>> GetPicketsByWarehouseAsync(int warehouseId, DateTime? time = null);

    /// <summary>
    /// Закрыть пикет
    /// </summary>
    /// <param name="id">Идентификатор пикета</param>
    /// <param name="closedAt">Время закрытия</param>
    /// <returns>Результат операции</returns>
    Task<bool> ClosePicketAsync(int id, DateTime? closedAt = null);
} 