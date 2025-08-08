using System.Net.Http;
using System.Threading.Tasks;
using Warehouses.client.Models.DTO;

namespace Warehouses.client.Services;

/// <summary>
/// Интерфейс для работы с API
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Базовый URL API
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// HTTP клиент для выполнения запросов
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Выполнить GET запрос
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных</typeparam>
    /// <param name="endpoint">Конечная точка API</param>
    /// <returns>Результат запроса</returns>
    Task<T?> GetAsync<T>(string endpoint);

    /// <summary>
    /// Выполнить POST запрос
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных</typeparam>
    /// <param name="endpoint">Конечная точка API</param>
    /// <param name="data">Данные для отправки</param>
    /// <returns>Результат запроса</returns>
    Task<T?> PostAsync<T>(string endpoint, object data);

    /// <summary>
    /// Выполнить PUT запрос
    /// </summary>
    /// <param name="endpoint">Конечная точка API</param>
    /// <param name="data">Данные для отправки</param>
    /// <returns>Результат запроса</returns>
    Task<bool> PutAsync(string endpoint, object data);

    /// <summary>
    /// Выполнить DELETE запрос
    /// </summary>
    /// <param name="endpoint">Конечная точка API</param>
    /// <returns>Результат запроса</returns>
    Task<bool> DeleteAsync(string endpoint);
} 