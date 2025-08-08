using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Warehouses.client.ViewModels.Base;

/// <summary>
/// Базовый класс для ViewModels с функционалом загрузки данных
/// </summary>
public abstract class LoadingViewModelBase : ViewModelBase
{
    protected readonly ILogger _logger;
    
    protected LoadingViewModelBase(ILogger logger)
    {
        _logger = logger;
    }
    
    protected async Task<bool> ExecuteWithLoadingAsync(Func<Task> operation, string loadingText, string errorMessage)
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            _logger.LogInformation("Начинаем выполнение операции: {Operation}", loadingText);
            
            await operation();
            
            _logger.LogInformation("Операция выполнена успешно: {Operation}", loadingText);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении операции: {Operation}", loadingText);
            SetError($"{errorMessage}: {ex.Message}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    protected async Task<T?> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string loadingText, string errorMessage)
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            _logger.LogInformation("Начинаем выполнение операции: {Operation}", loadingText);
            
            var result = await operation();
            
            _logger.LogInformation("Операция выполнена успешно: {Operation}", loadingText);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении операции: {Operation}", loadingText);
            SetError($"{errorMessage}: {ex.Message}");
            return default(T);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
