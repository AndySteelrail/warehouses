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
public partial class CloseWarehouseViewModel : CloseItemViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly Warehouse _warehouse;
    
    public CloseWarehouseViewModel(
        IWarehouseService warehouseService,
        IDialogService dialogService,
        ILogger<CloseWarehouseViewModel> logger,
        Warehouse warehouse) : base(dialogService, logger)
    {
        _warehouseService = warehouseService;
        _warehouse = warehouse;
    }
    
    public string WarehouseName => _warehouse.Name;
    public override string WindowTitle => "Закрытие склада";
    public override string ConfirmText => "Вы действительно хотите закрыть склад?";
    public override string TargetName => _warehouse.Name;
    
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
        await ExecuteWithLoadingAsync(async () =>
        {
            var success = await _warehouseService.CloseWarehouseAsync(_warehouse.Id, GetCreatedAtUtc());
            if (!success)
            {
                throw new Exception("Не удалось закрыть склад");
            }

            var message = $"Склад '{_warehouse.Name}' успешно закрыт";
            if (ClosedAt != DateTime.Now)
            {
                message += $" на время {ClosedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);
            CloseWindow(true);
            return true;
        }, "Закрытие склада", "Ошибка при закрытии склада");
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }


}
