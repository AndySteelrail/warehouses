using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Warehouses.client.Models;
using Warehouses.client.Services;
using Warehouses.client.ViewModels.Base;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для управления списком складов
/// </summary>
public partial class WarehousesViewModel : LoadingViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private ObservableCollection<Warehouse> _warehouses = new();
    private Warehouse? _selectedWarehouse;
    private string _newWarehouseName = string.Empty;
    
    public WarehousesViewModel(IWarehouseService warehouseService, ILogger<WarehousesViewModel> logger)
        : base(logger)
    {
        _warehouseService = warehouseService;
        LoadWarehousesCommand.ExecuteAsync(null);
    }
    
    public ObservableCollection<Warehouse> Warehouses
    {
        get => _warehouses;
        set => SetProperty(ref _warehouses, value);
    }
    
    public Warehouse? SelectedWarehouse
    {
        get => _selectedWarehouse;
        set => SetProperty(ref _selectedWarehouse, value);
    }
    
    public string NewWarehouseName
    {
        get => _newWarehouseName;
        set => SetProperty(ref _newWarehouseName, value);
    }
    

    
    [RelayCommand]
    private async Task LoadWarehouses()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            Warehouses.Clear();
            
            foreach (var warehouse in warehouses)
            {
                Warehouses.Add(warehouse);
            }
        }, "Загрузка складов", "Ошибка при загрузке складов");
    }
    
    [RelayCommand]
    private async Task CreateWarehouse()
    {
        if (string.IsNullOrWhiteSpace(NewWarehouseName))
        {
            SetError("Введите название склада");
            return;
        }

        var warehouse = await ExecuteWithLoadingAsync(
            () => _warehouseService.CreateWarehouseAsync(NewWarehouseName.Trim()),
            "Создание склада",
            "Ошибка при создании склада");
            
        if (warehouse != null)
        {
            Warehouses.Add(warehouse);
            NewWarehouseName = string.Empty;
        }
        else
        {
            SetError("Не удалось создать склад");
        }
    }
    

} 