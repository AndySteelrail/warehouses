using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для закрытия склада
/// </summary>
public partial class CloseWarehouseViewModel : DateTimeViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<CloseWarehouseViewModel> _logger;
    private readonly Warehouse _warehouse;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CloseWarehouseViewModel(
        IWarehouseService warehouseService,
        IDialogService dialogService,
        ILogger<CloseWarehouseViewModel> logger,
        Warehouse warehouse) : base(dialogService)
    {
        _warehouseService = warehouseService;
        _logger = logger;
        _warehouse = warehouse;
    }
    
    public string WarehouseName => _warehouse.Name;
    
    public DateTime ClosedAt
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }
    
    public string ClosedAtText
    {
        get => CreatedAtText;
        set => CreatedAtText = value;
    }
    
    [RelayCommand]
    private async Task Close()
    {
        await ExecuteWithOverlayAsync(async () =>
        {
            var success = await _warehouseService.CloseWarehouseAsync(_warehouse.Id, GetCreatedAtUtc());
            if (!success)
            {
                throw new Exception("Не удалось закрыть склад");
            }

            _logger.LogInformation("Склад успешно закрыт: Id={Id}, Name={Name}, ClosedAt={ClosedAt}",
                _warehouse.Id, _warehouse.Name, ClosedAt);

            var message = $"Склад '{_warehouse.Name}' успешно закрыт";
            if (ClosedAt != DateTime.Now)
            {
                message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);
            CloseWindow(true);
        }, "Закрытие склада", "Ошибка при закрытии склада");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }

    private async Task<bool> ExecuteWithOverlayAsync(Func<Task> action, string loadingText, string errorPrefix)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            await action();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorPrefix);
            await ShowErrorAsync($"{errorPrefix}: {ex.Message}");
            return false;
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
}
