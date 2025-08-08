using System;
using Warehouses.client.Models;

namespace Warehouses.client.ViewModels;

/// <summary>
/// ViewModel для отображения деталей элемента
/// </summary>
public partial class ItemDetailsViewModel : ViewModelBase
{
    private TreeNode? _selectedNode;

    /// <summary>
    /// Выбранный узел
    /// </summary>
    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetProperty(ref _selectedNode, value);
            UpdateDetails();
        }
    }

    #region Свойства для отображения

    // Склад
    public bool IsWarehouse => SelectedNode?.NodeType == TreeNodeType.Warehouse;
    public string WarehouseId => SelectedNode?.Data is Warehouse warehouse ? warehouse.Id.ToString() : "";
    public string WarehouseName => SelectedNode?.Data is Warehouse warehouse ? warehouse.Name : "";

    // Площадка
    public bool IsPlatform => SelectedNode?.NodeType == TreeNodeType.Platform;
    public string PlatformId => SelectedNode?.Data is Platform platform ? platform.Id.ToString() : "";
    public string PlatformName => SelectedNode?.Data is Platform platform ? platform.Name : "";
    public string PlatformStatus => SelectedNode?.Data is Platform platform ? (platform.IsActive ? "Активна" : "Закрыта") : "";
    public string PlatformCreatedAt => SelectedNode?.Data is Platform platform ? platform.CreatedAt.ToString("dd.MM.yyyy HH:mm") : "";

    // Пикет
    public bool IsPicket => SelectedNode?.NodeType == TreeNodeType.Picket;
    public string PicketId => SelectedNode?.Data is Picket picket ? picket.Id.ToString() : "";
    public string PicketName => SelectedNode?.Data is Picket picket ? picket.Name : "";

    // Груз
    public bool HasCargo => SelectedNode?.Data is 
        Platform platform && platform.CurrentCargo != null;
    public string CargoType => SelectedNode?.Data is 
        Platform platform && platform.CurrentCargo != null ? platform.CurrentCargo.GoodType : "";
    public string CargoRemainder => SelectedNode?.Data is 
        Platform platform && platform.CurrentCargo != null ? platform.CurrentCargo.Remainder.ToString() : "";
    public string CargoUnit => "т.";
    public string CargoComing => SelectedNode?.Data is 
        Platform platform && platform.CurrentCargo != null ? platform.CurrentCargo.Coming.ToString() : "";
    public string CargoConsumption => SelectedNode?.Data is 
        Platform platform && platform.CurrentCargo != null ? platform.CurrentCargo.Consumption.ToString() : "";

    // Общие
    public bool IsEmpty => SelectedNode == null;

    #endregion

    /// <summary>
    /// Обновить детали при изменении выбранного узла
    /// </summary>
    private void UpdateDetails()
    {
        OnPropertyChanged(nameof(IsWarehouse));
        OnPropertyChanged(nameof(WarehouseId));
        OnPropertyChanged(nameof(WarehouseName));
        
        OnPropertyChanged(nameof(IsPlatform));
        OnPropertyChanged(nameof(PlatformId));
        OnPropertyChanged(nameof(PlatformName));
        OnPropertyChanged(nameof(PlatformStatus));
        OnPropertyChanged(nameof(PlatformCreatedAt));
        
        OnPropertyChanged(nameof(IsPicket));
        OnPropertyChanged(nameof(PicketId));
        OnPropertyChanged(nameof(PicketName));
        
        OnPropertyChanged(nameof(HasCargo));
        OnPropertyChanged(nameof(CargoType));
        OnPropertyChanged(nameof(CargoRemainder));
        OnPropertyChanged(nameof(CargoUnit));
        OnPropertyChanged(nameof(CargoComing));
        OnPropertyChanged(nameof(CargoConsumption));
        
        OnPropertyChanged(nameof(IsEmpty));
    }
} 