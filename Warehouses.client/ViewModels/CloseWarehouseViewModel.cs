using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для закрытия склада
/// </summary>
public partial class CloseWarehouseViewModel : ViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<CloseWarehouseViewModel> _logger;
    private readonly Warehouse _warehouse;
    
    private DateTime _closedAt = DateTime.Now;
    private string _closedAtText;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CloseWarehouseViewModel(
        IWarehouseService warehouseService,
        IDialogService dialogService,
        ILogger<CloseWarehouseViewModel> logger,
        Warehouse warehouse)
    {
        _warehouseService = warehouseService;
        _dialogService = dialogService;
        _logger = logger;
        _warehouse = warehouse;
        _closedAtText = _closedAt.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public string WarehouseName => _warehouse.Name;
    
    public DateTime ClosedAt
    {
        get => _closedAt;
        set => SetProperty(ref _closedAt, value);
    }
    
    public string ClosedAtText
    {
        get => _closedAtText;
        set
        {
            if (SetProperty(ref _closedAtText, value))
            {
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    _closedAt = parsedDate;
                }
            }
        }
    }
    
    public event Action<bool>? WindowClosed;
    
    [RelayCommand]
    private async Task Close()
    {
        try
        {
            LoadingOverlay.LoadingText = "Закрытие склада...";
            LoadingOverlay.IsVisible = true;
            ClearError();

            var success = await _warehouseService.CloseWarehouseAsync(_warehouse.Id, ClosedAt);
            
            if (success)
            {
                _logger.LogInformation("Склад успешно закрыт: Id={Id}, Name={Name}, ClosedAt={ClosedAt}", 
                    _warehouse.Id, _warehouse.Name, ClosedAt);
                
                var message = $"Склад '{_warehouse.Name}' успешно закрыт";
                if (ClosedAt != DateTime.Now)
                {
                    message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
                }
                await _dialogService.ShowMessageAsync("Успех", message);
                
                CloseWindow(true);
            }
            else
            {
                SetError("Не удалось закрыть склад");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии склада: {Message}", ex.Message);
            SetError($"Ошибка при закрытии склада: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
    
    private void CloseWindow(bool result)
    {
        WindowClosed?.Invoke(result);
    }
}
