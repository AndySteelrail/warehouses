using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс сервиса для работы со справочниками
/// </summary>
/// <summary>
/// Интерфейс сервиса для работы со справочными данными
/// </summary>
public interface IReferenceService
{
    /// <summary>
    /// Получить все типы грузов
    /// </summary>
    /// <returns>Список типов грузов</returns>
    Task<List<CargoType>> GetCargoTypesAsync();


} 