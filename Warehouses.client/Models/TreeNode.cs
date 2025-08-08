using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Warehouses.client.Models;

/// <summary>
/// Узел древовидной структуры для отображения складов, площадок и пикетов
/// </summary>
public partial class TreeNode : ObservableObject
{
    private string _displayName = string.Empty;
    private string _cargoInfo = string.Empty;
    private bool _canAddCargo;
    private TreeNodeType _nodeType;
    private int _id;
    private int _parentId;
    private string _cargoType = string.Empty;
    private decimal _cargoAmount;
    private bool _canEdit;
    private bool _canDelete;
    private bool _canCreatePicket;
    private bool _canCreatePlatform;
    
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }
    
    public string CargoInfo
    {
        get => _cargoInfo;
        set => SetProperty(ref _cargoInfo, value);
    }
    
    public bool CanAddCargo
    {
        get => _canAddCargo;
        set => SetProperty(ref _canAddCargo, value);
    }
    
    public bool CanEdit
    {
        get => _canEdit;
        set => SetProperty(ref _canEdit, value);
    }
    
    public bool CanDelete
    {
        get => _canDelete;
        set => SetProperty(ref _canDelete, value);
    }
    
    public bool CanCreatePicket
    {
        get => _canCreatePicket;
        set => SetProperty(ref _canCreatePicket, value);
    }
    
    public bool CanCreatePlatform
    {
        get => _canCreatePlatform;
        set => SetProperty(ref _canCreatePlatform, value);
    }
    
    public TreeNodeType NodeType
    {
        get => _nodeType;
        set => SetProperty(ref _nodeType, value);
    }
    
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    public int ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }
    
    public string CargoType
    {
        get => _cargoType;
        set => SetProperty(ref _cargoType, value);
    }
    
    public decimal CargoAmount
    {
        get => _cargoAmount;
        set => SetProperty(ref _cargoAmount, value);
    }
    
    public bool IsPlatform => NodeType == TreeNodeType.Platform;
    
    public ObservableCollection<TreeNode> Children { get; set; } = new();
    
    public object? Data { get; set; }
    
    public Func<int, Task<bool>>? AddCargoCallback { get; set; }
    
    public Func<TreeNode, Task<bool>>? EditCallback { get; set; }
    
    public Func<TreeNode, Task<bool>>? DeleteCallback { get; set; }
    
    public Func<TreeNode, Task<bool>>? CreatePicketCallback { get; set; }
    
    public Func<TreeNode, Task<bool>>? CreatePlatformCallback { get; set; }
    
    [RelayCommand]
    private async Task EditAsync()
    {
        if (EditCallback != null)
        {
            await EditCallback(this);
        }
    }
    
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (DeleteCallback != null)
        {
            await DeleteCallback(this);
        }
    }
    
    [RelayCommand]
    private async Task AddCargoAsync()
    {
        if (AddCargoCallback != null && NodeType == TreeNodeType.Platform)
        {
            await AddCargoCallback(Id);
        }
    }
    
    [RelayCommand]
    private async Task CreatePicketAsync()
    {
        if (CreatePicketCallback != null)
        {
            await CreatePicketCallback(this);
        }
    }
    
    [RelayCommand]
    private async Task CreatePlatformAsync()
    {
        if (CreatePlatformCallback != null)
        {
            await CreatePlatformCallback(this);
        }
    }
}

/// <summary>
/// Тип узла в дереве
/// </summary>
public enum TreeNodeType
{
    Warehouse,
    Platform,
    Picket,
    
    // Кнопка создания склада
    CreateWarehouse
} 