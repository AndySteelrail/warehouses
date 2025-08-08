using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для создания склада
/// </summary>
public partial class CreateWarehouseViewModel : ViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<CreateWarehouseViewModel> _logger;
    private string _warehouseName = string.Empty;
    private DateTime _createdAt = DateTime.Now;
    private string _createdAtText = string.Empty;
    
    public LoadingOverlayViewModel LoadingOverlay { get; } = new();
    
    public CreateWarehouseViewModel(IWarehouseService warehouseService, IDialogService dialogService, ILogger<CreateWarehouseViewModel> logger)
    {
        _warehouseService = warehouseService;
        _dialogService = dialogService;
        _logger = logger;
        
        _createdAtText = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public string WarehouseName
    {
        get => _warehouseName;
        set
        {
            SetProperty(ref _warehouseName, value);
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }
    
    public string CreatedAtText
    {
        get => _createdAtText;
        set
        {
            if (SetProperty(ref _createdAtText, value))
            {
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    _createdAt = parsedDate;
                }
            }
        }
    }
    
    public bool CanCreate => !string.IsNullOrWhiteSpace(WarehouseName);
    
    public event Action<bool>? WindowClosed;
    
    [RelayCommand]
    private async Task Create()
    {
        if (!CanCreate)
        {
            SetError("Введите название склада");
            return;
        }

        try
        {
            LoadingOverlay.LoadingText = "Создание склада...";
            LoadingOverlay.IsVisible = true;
            ClearError();

            var warehouse = await _warehouseService.CreateWarehouseAsync(WarehouseName.Trim(), CreatedAt);
            if (warehouse != null)
            {
                _logger.LogInformation("Склад успешно создан: Id={Id}, Name={Name}", warehouse.Id, warehouse.Name);
                
                var message = $"Склад '{warehouse.Name}' успешно создан";
                if (CreatedAt != DateTime.Now)
                {
                    message += $" на время {CreatedAt:yyyy-MM-dd HH:mm:ss}";
                }
                await _dialogService.ShowMessageAsync("Успех", message);
                
                CloseWindow(true);
            }
            else
            {
                SetError("Не удалось создать склад");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при создании склада: {ex.Message}");
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