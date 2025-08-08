using System;
using System.Threading.Tasks;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы с грузами
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы с грузами
/// </summary>
public interface ICargoService
{
    /// <summary>
    /// Добавить операцию с грузом (приход или расход)
    /// </summary>
    /// <param name="platformId">ID площадки</param>
    /// <param name="cargoTypeId">ID типа груза</param>
    /// <param name="coming">Количество прихода (null для расхода)</param>
    /// <param name="consumption">Количество расхода (null для прихода)</param>
    /// <param name="recordedAt">Время записи операции</param>
    /// <returns>Task</returns>
    Task AddCargoOperationAsync(int platformId, int cargoTypeId, decimal? coming = null, decimal? consumption = null, DateTime? recordedAt = null);


} 