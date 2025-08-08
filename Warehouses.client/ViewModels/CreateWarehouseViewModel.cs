using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для создания склада
/// </summary>
public partial class CreateWarehouseViewModel : NameViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<CreateWarehouseViewModel> _logger;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CreateWarehouseViewModel(IWarehouseService warehouseService, IDialogService dialogService, ILogger<CreateWarehouseViewModel> logger)
        : base(dialogService)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }
    
    /// <summary>
    /// Имя склада (переопределяем свойство Name из базового класса)
    /// </summary>
    public string WarehouseName
    {
        get => Name;
        set => Name = value;
    }
    
    [RelayCommand]
    private async Task Create()
    {
        if (!ValidateName())
            return;

        var executed = await ExecuteWithLoadingAsync(async () =>
        {
            var warehouse = await _warehouseService.CreateWarehouseAsync(GetCleanedName(), GetCreatedAtUtc());
            if (warehouse == null)
            {
                throw new Exception("Не удалось создать склад");
            }

            _logger.LogInformation("Склад успешно создан: Id={Id}, Name={Name}", warehouse.Id, warehouse.Name);

            var message = $"Склад '{warehouse.Name}' успешно создан";
            if (CreatedAt != DateTime.Now)
            {
                message += $" на время {CreatedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);

            CloseWindow(true);
        }, "Создание склада", "Ошибка при создании склада");
    }

    private async Task<bool> ExecuteWithLoadingAsync(Func<Task> operation, string loadingText, string errorMessage)
    {
        try
        {
            LoadingOverlay.LoadingText = loadingText;
            LoadingOverlay.IsVisible = true;
            ClearError();
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            SetError($"{errorMessage}: {ex.Message}");
            return false;
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
} 