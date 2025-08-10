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
public partial class CreateWarehouseViewModel : FormViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    
    public CreateWarehouseViewModel(IWarehouseService warehouseService, IDialogService dialogService, ILogger<CreateWarehouseViewModel> logger)
        : base(logger, dialogService)
    {
        _warehouseService = warehouseService;
        
        // Подписываемся на изменения базовых свойств для обновления CanCreate
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(CreatedAt) || e.PropertyName == nameof(CreatedAtText))
            {
                OnPropertyChanged(nameof(CanCreate));
            }
        };
    }
    
    /// <summary>
    /// Имя склада (переопределяем свойство Name из базового класса)
    /// </summary>
    public string WarehouseName
    {
        get => Name;
        set
        {
            Name = value;
            OnPropertyChanged(nameof(CanCreate));
        }
    }
    
    [RelayCommand]
    private async Task Create()
    {
        if (!CanCreate)
        {
            SetError("Заполните все обязательные поля");
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var warehouse = await _warehouseService.CreateWarehouseAsync(GetCleanedName(), GetCreatedAtUtc());
            if (warehouse == null)
            {
                throw new Exception("Не удалось создать склад");
            }

            var message = $"Склад '{warehouse.Name}' успешно создан";
            if (CreatedAt != DateTime.Now)
            {
                message += $" на время {CreatedAt:yyyy-MM-dd HH:mm:ss}";
            }
            await ShowSuccessAsync(message);

            CloseWindow(true);
            return true;
        }, "Создание склада", "Ошибка при создании склада");
    }


    
    public bool CanCreate => !string.IsNullOrWhiteSpace(WarehouseName);
    
    [RelayCommand]
    private void Cancel()
    {
        CloseWindow(false);
    }
} 