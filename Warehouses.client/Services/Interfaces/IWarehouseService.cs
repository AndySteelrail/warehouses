using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouses.client.Models;
using Warehouses.client.Models.DTO;
using Warehouses.client.Models.DTO.Tree;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы со складами
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы со складами
/// </summary>
public interface IWarehouseService
{
    /// <summary>
    /// Получить все склады
    /// </summary>
    /// <param name="time">Время для фильтрации (опционально)</param>
    /// <returns>Список складов</returns>
    Task<List<Warehouse>> GetAllWarehousesAsync(DateTime? time = null);
    Task<WarehousesTreeDTO> GetWarehousesTreeAsync(DateTime time, int? cargoTypeId = null);



    /// <summary>
    /// Создать новый склад
    /// </summary>
    /// <param name="name">Название склада</param>
    /// <param name="createdAt">Время создания</param>
    /// <returns>Созданный склад</returns>
    Task<Warehouse?> CreateWarehouseAsync(string name, DateTime? createdAt = null);

    /// <summary>
    /// Обновить склад
    /// </summary>
    /// <param name="id">Идентификатор склада</param>
    /// <param name="name">Новое название склада</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpdateWarehouseAsync(int id, string name);

    /// <summary>
    /// Удалить склад
    /// </summary>
    /// <param name="id">Идентификатор склада</param>
    /// <returns>Результат операции</returns>


    /// <summary>
    /// Закрыть склад (вместе со всеми площадками и пикетами)
    /// </summary>
    /// <param name="id">Идентификатор склада</param>
    /// <param name="closedAt">Время закрытия</param>
    /// <returns>Результат операции</returns>
    Task<bool> CloseWarehouseAsync(int id, DateTime? closedAt = null);
} 