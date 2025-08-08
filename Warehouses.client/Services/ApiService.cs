using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;

namespace Warehouses.client.Services;

/// <summary>
/// Реализация сервиса для работы с API
/// </summary>
public class ApiService : IApiService
{
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        HttpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public string BaseUrl => "api/";
    
    public HttpClient HttpClient { get; }
    
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var fullUrl = $"{BaseUrl}{endpoint}";
            _logger.LogInformation("Выполняем GET запрос к {Url}", fullUrl);
            
            var response = await HttpClient.GetAsync(fullUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GET запрос успешен, получено {Length} символов", content.Length);
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GET запрос к {Endpoint} завершился с кодом {StatusCode}, ответ: {Error}", endpoint, response.StatusCode, errorContent);
            
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
            if (errorResponse?.Message != null)
            {
                throw new HttpRequestException(errorResponse.Message);
            }
            
            throw new HttpRequestException("Произошла ошибка при выполнении запроса");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении GET запроса к {Endpoint}: {Message}", endpoint, ex.Message);
            throw;
        }
    }
    
    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var fullUrl = $"{BaseUrl}{endpoint}";
            _logger.LogInformation("Выполняем POST запрос к {Endpoint}", fullUrl);
            _logger.LogInformation("Отправляемые данные: {Data}", JsonSerializer.Serialize(data));
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await HttpClient.PostAsync(fullUrl, content);
            
            _logger.LogInformation("POST запрос к {Endpoint} завершился с кодом {StatusCode}", endpoint, response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Получен ответ: {ResponseContent}", responseContent);
                
                if (string.IsNullOrEmpty(responseContent))
                {
                    _logger.LogInformation("Получен пустой ответ, возвращаем default");
                    return default(T);
                }
                
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                _logger.LogInformation("Данные успешно десериализованы в тип {Type}", typeof(T).Name);
                return result;
            }
            
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("POST запрос к {Endpoint} завершился с ошибкой {StatusCode}: {ErrorContent}", endpoint, response.StatusCode, errorBody);
            if (TryGetErrorMessage(errorBody, out var postErr))
            {
                throw new HttpRequestException(postErr);
            }
            throw new HttpRequestException("Произошла ошибка при выполнении запроса");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении POST запроса к {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<bool> PutAsync(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await HttpClient.PutAsync($"{BaseUrl}{endpoint}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("PUT запрос к {Endpoint} завершился с кодом {StatusCode}: {ErrorContent}", endpoint, response.StatusCode, errorBody);
            if (TryGetErrorMessage(errorBody, out var putErr))
            {
                throw new HttpRequestException(putErr);
            }
            throw new HttpRequestException("Произошла ошибка при выполнении запроса");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении PUT запроса к {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}{endpoint}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("DELETE запрос к {Endpoint} завершился с кодом {StatusCode}: {ErrorContent}", endpoint, response.StatusCode, errorBody);
            if (TryGetErrorMessage(errorBody, out var delErr))
            {
                throw new HttpRequestException(delErr);
            }
            throw new HttpRequestException("Произошла ошибка при выполнении запроса");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении DELETE запроса к {Endpoint}", endpoint);
            throw;
        }
    }

    private bool TryGetErrorMessage(string errorContent, out string message)
    {
        message = string.Empty;
        try
        {
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
            if (!string.IsNullOrWhiteSpace(errorResponse?.Message))
            {
                message = errorResponse!.Message!;
                return true;
            }
        }
        catch (JsonException)
        {
        }
        return false;
    }
} 