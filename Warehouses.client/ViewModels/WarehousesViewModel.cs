using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Warehouses.client.Models;
using Warehouses.client.Services;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для управления списком складов
/// </summary>
public partial class WarehousesViewModel : ViewModelBase
{
    private readonly IWarehouseService _warehouseService;
    private ObservableCollection<Warehouse> _warehouses = new();
    private Warehouse? _selectedWarehouse;
    private string _newWarehouseName = string.Empty;
    
    public WarehousesViewModel(IWarehouseService warehouseService)
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
        try
        {
            IsBusy = true;
            ClearError();

            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            Warehouses.Clear();
            
            foreach (var warehouse in warehouses)
            {
                Warehouses.Add(warehouse);
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при загрузке складов: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task CreateWarehouse()
    {
        if (string.IsNullOrWhiteSpace(NewWarehouseName))
        {
            SetError("Введите название склада");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var warehouse = await _warehouseService.CreateWarehouseAsync(NewWarehouseName.Trim());
            
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
        catch (Exception ex)
        {
            SetError($"Ошибка при создании склада: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    

} 